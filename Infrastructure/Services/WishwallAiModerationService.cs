using System.Net;
using Domain.Enums;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Application.Interfaces;
using Domain.Entity;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Domain.Interface;
using Domain.Interfaces;

namespace Infrastructure.Services
{
    public class WishwallAiModerationService : IWishwallAiModerationService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<WishwallAiModerationService> _logger;

        public WishwallAiModerationService(
            IServiceScopeFactory scopeFactory,
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            ILogger<WishwallAiModerationService> logger)
        {
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        public Task EnqueueModerationAsync(Guid messageId, string message, CancellationToken ct = default)
        {
            _ = Task.Run(async () => 
            {
                try 
                {
                    await ModerateAndUpdateAsync(messageId, message);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "[AI-FATAL] Background Task Crashed for {MessageId}", messageId);
                }
            }, CancellationToken.None);
            return Task.CompletedTask;
        }

        private async Task ModerateAndUpdateAsync(Guid messageId, string message)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var label = "ALLOW";
                var reason = "fast:clean";
                var source = "fast";

                _logger.LogInformation("[AI] Processing message {MessageId}: '{Message}'", messageId, message);

                if (IsFastBlocked(message))
                {
                    label = "BLOCK";
                    reason = "fast:keyword";
                    _logger.LogInformation("[AI] Fast-Blocked by keyword: '{Message}'", message);
                }
                else
                {
                    var ai = await CallGeminiAsync(message);
                    label = ai.Label;
                    reason = ai.Reason;
                    source = ai.Source;
                    _logger.LogInformation("[AI] Gemini result: Label={Label}, Reason={Reason}", label, reason);
                }

                var msg = await db.WishwallMessages
                    .Include(m => m.User)
                    .FirstOrDefaultAsync(m => m.Id == messageId);

                if (msg != null)
                {
                    msg.AiLabel = label;
                    msg.AiReason = reason;

                    // Sync with sentiment for Frontend display
                    if (label == "ALLOW") msg.Sentiment = WishwallSentiment.Positive;
                    else if (label == "BLOCK") msg.Sentiment = WishwallSentiment.Negative;
                    else msg.Sentiment = WishwallSentiment.Neutral;

                    if (label == "BLOCK")
                    {
                        msg.IsHidden = true;
                    }
                }

                if (label != "ALLOW")
                {
                    db.WishwallAiLogs.Add(new WishwallAiLog
                    {
                        MessageId = messageId,
                        Label = label,
                        Reason = reason,
                        Source = source,
                        DurationMs = (int)sw.ElapsedMilliseconds
                    });
                }

                await db.SaveChangesAsync();

                // Only notify staff if message found
                if (msg != null)
                {
                    var notifier = scope.ServiceProvider.GetRequiredService<IWishwallNotifier>();

                    // Notify Staff about the new AI Log (Real-time)
                    await notifier.NotifyStaffNewAiLogAsync(msg.EventId, new
                    {
                        messageId = messageId,
                        message = msg.Message,
                        label = label,
                        reason = reason,
                        source = source,
                        createdAt = DateTime.UtcNow
                    });

                    // Only notify staff about pending review if NOT blocked
                    if (label != "BLOCK")
                    {
                        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                        var encryption = scope.ServiceProvider.GetRequiredService<IEncryptionService>();

                        var user = await userRepo.GetByIdWithoutDecryptAsync(msg.UserId);
                        var userName = user != null ? encryption.Decrypt(user.Name) : "Anonymous";

                        await notifier.NotifyStaffNewPendingAsync(msg.EventId, new
                        {
                            id = msg.Id,
                            userName = userName,
                            message = msg.Message,
                            sentiment = msg.Sentiment.ToString(),
                            aiLabel = label,
                            aiReason = reason,
                            createdAt = msg.CreatedAt
                        });
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[AI] Moderation timed out or cancelled for message {MessageId}", messageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AI] FATAL ERROR during moderation for message {MessageId}", messageId);
            }
            finally
            {
                _logger.LogInformation("[AI] Finished moderation for {MessageId} in {Elapsed}ms", messageId, sw.ElapsedMilliseconds);
            }
        }

        private bool IsFastBlocked(string message)
        {
            var lower = message.ToLowerInvariant();

            var keywords = _config.GetSection("WishwallModeration:FastBlockKeywords").Get<string[]>() ?? Array.Empty<string>();
            _logger.LogInformation("Loaded {Count} FastBlockKeywords. Testing: '{Message}'. Keywords: [{Keywords}]", keywords.Length, message, string.Join("|", keywords));
            foreach (var k in keywords)
            {
                if (!string.IsNullOrWhiteSpace(k) && lower.Contains(k.Trim().ToLowerInvariant()))
                {
                    _logger.LogInformation("MATCH: Keyword '{Keyword}' found in '{Message}'", k, message);
                    return true;
                }
            }

            var patterns = _config.GetSection("WishwallModeration:FastBlockRegex").Get<string[]>() ?? Array.Empty<string>();
            foreach (var p in patterns)
            {
                if (!string.IsNullOrWhiteSpace(p) && Regex.IsMatch(message, p, RegexOptions.IgnoreCase | RegexOptions.Compiled))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<(string Label, string Reason, string Source)> CallGeminiAsync(string message)
        {
            var enabled = _config.GetValue("WishwallModeration:Enabled", true);
            if (!enabled)
            {
                return ("WARNING", "ai:disabled", "fallback");
            }

            var apiKey = _config["Gemini:ApiKey"];
            var model = _config["Gemini:Model"];
            var endpoint = _config["Gemini:Endpoint"];
            var timeoutMs = _config.GetValue("Gemini:TimeoutMs", 2000);

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(model))
            {
                _logger.LogWarning("[AI] Missing API Key or Model Name. Key starts with: {Prefix}", (apiKey ?? "").Take(5).ToString());
                return ("REVIEW", "ai:missing_key_or_model", "fallback");
            }

            var url = string.IsNullOrWhiteSpace(endpoint)
                ? $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}"
                : endpoint.Replace("{MODEL}", model, StringComparison.OrdinalIgnoreCase).Replace("{API_KEY}", apiKey, StringComparison.OrdinalIgnoreCase);

            var debugUrl = url.Length > 20 ? url.Substring(0, url.Length - 10) + "**********" : url;
            _logger.LogInformation("[AI] Calling Gemini: {Url}", debugUrl);

            var prompt = BuildCompactPrompt(message);

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = prompt } }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.0,
                    maxOutputTokens = 12
                }
            };

            var json = JsonSerializer.Serialize(payload);
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
            var client = _httpClientFactory.CreateClient("Gemini");

            using var resp = await client.SendAsync(req, cts.Token);
            if (!resp.IsSuccessStatusCode)
            {
                var errorBody = await resp.Content.ReadAsStringAsync(cts.Token);
                _logger.LogError("[AI] Gemini API Error (Status {Status}): {Body}", resp.StatusCode, errorBody);
                
                // Fallback to internal keyword/sentiment scan if API fails
                return InternalFallbackScan(message);
            }

            var body = await resp.Content.ReadAsStringAsync(cts.Token);
            var text = ExtractText(body);
            var label = ParseLabel(text);
            return (label, "ai:gemini", "gemini");
        }

        private (string Label, string Reason, string Source) InternalFallbackScan(string message)
        {
            var lower = message.ToLowerInvariant();
            
            // Comprehensive internal patterns for common insults and teencode
            var toxicPatterns = new[] 
            { 
                "óc", "lợn", "heo", "chó", "cc", "cl", "dm", "dmm", "cmm", "bùi", "cac", "cak", 
                "vcl", "vkl", "vl", "ngu", "đĩ", "lồn", "cặc", "cút", "mẹ mày", "con chó", "đẻ",
                "thằng", "mẹ", "dở", "tệ", "kém", "vô dụng", "nát", "rác", "đấm vào tai", "ngáo"
            };

            foreach (var kw in toxicPatterns)
            {
                // Simple contains check is often more resilient for teencode like "óc!!!"
                if (lower.Contains(kw))
                {
                    return ("BLOCK", "fallback:super_filter", "internal");
                }
            }

            // Fallback for non-toxic but API error cases
            // Since Staff will review anyway, ALLOW is safer to show on UI than N/A
            return ("ALLOW", "fallback:api_error", "internal");
        }

        private static string BuildCompactPrompt(string message)
        {
            // Output must be single token: ALLOW, WARNING, or BLOCK
            return "Task: Professional Vietnamese content moderation for a public display.\n" +
                   "Context: Act as a Vietnamese linguistic expert. Professional and strict.\n" +
                   "Categories:\n" +
                   "- BLOCK: Profanity, animal-based insults, OR ANY NEGATIVE CRITICISM/INSULTS (e.g., 'hát dở', 'vô dụng', 'tệ', 'kém', 'đấm vào tai').\n" +
                   "- WARNING: Suspicious teencode or sarcasm.\n" +
                   "- ALLOW: Clean, polite, and positive wishes ONLY.\n" +
                   "Goal: BLOCK ALL negative, insulting or mean-spirited comments. Focus on maintaining a positive atmosphere.\n" +
                   "Message: " + message + "\n" +
                   "Output ONLY ONE WORD (ALLOW, WARNING, or BLOCK):";
        }

        private static string ExtractText(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                {
                    return string.Empty;
                }

                var content = candidates[0].GetProperty("content");
                if (!content.TryGetProperty("parts", out var parts) || parts.GetArrayLength() == 0)
                {
                    return string.Empty;
                }

                var text = parts[0].GetProperty("text").GetString();
                return text ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string ParseLabel(string text)
        {
            var t = (text ?? string.Empty).Trim().ToUpperInvariant();
            if (t.Contains("BLOCK")) return "BLOCK";
            if (t.Contains("WARNING")) return "WARNING";
            if (t.Contains("ALLOW")) return "ALLOW";
            return "WARNING";
        }
    }
}
