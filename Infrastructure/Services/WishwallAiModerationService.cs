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

                var msg = await db.WishwallMessages
                    .Include(m => m.User)
                    .FirstOrDefaultAsync(m => m.Id == messageId);

                // Khôi phục bộ lọc Fast-Blocked để giảm tải cho API (vì đang bị Quota Error)
                if (IsFastBlocked(message))
                {
                    label = "BLOCK";
                    reason = "fast:keyword";
                    _logger.LogInformation("[AI] Fast-Blocked by keyword: '{Message}'", message);
                }
                // Thử gọi AI (Gemini hoặc backup sang Groq)
                var ai = await ModerateWithAiAsync(messageId, message, msg?.EventId ?? Guid.Empty);
                label = ai.Label;
                reason = ai.Reason;
                source = ai.Source;
                _logger.LogInformation("[AI] Final result: Label={Label}, Source={Source}", label, source);

                if (msg != null)
                {
                    msg.AiLabel = label;
                    msg.AiReason = reason;

                    // Sync with sentiment for Frontend display
                    // Default to Neutral instead of Positive for ALLOW to avoid "NỔI BẬT" everywhere
                    if (label == "BLOCK") msg.Sentiment = WishwallSentiment.Negative;
                    else if (label == "ALLOW" && reason == "ai:gemini") 
                    {
                        // Temporarily keep Neutral, or we can add logic for Positive later
                        msg.Sentiment = WishwallSentiment.Neutral; 
                    }
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
            if (string.IsNullOrWhiteSpace(message)) return false;

            // Normalize to FormC for consistent Vietnamese matching
            var normalizedMessage = message.Normalize(System.Text.NormalizationForm.FormC);
            var lower = normalizedMessage.ToLowerInvariant();

            var keywords = _config.GetSection("WishwallModeration:FastBlockKeywords").Get<string[]>() ?? Array.Empty<string>();
            _logger.LogInformation("[AI-Fast] Testing message: '{Message}'. Keywords Count: {Count}", message, keywords.Length);

            foreach (var k in keywords)
            {
                if (string.IsNullOrWhiteSpace(k)) continue;
                
                var normalizedK = k.Normalize(System.Text.NormalizationForm.FormC).Trim().ToLowerInvariant();
                if (lower.Contains(normalizedK))
                {
                    _logger.LogInformation("[AI-Fast] MATCH: Keyword '{Keyword}' found in '{Message}'", k, message);
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
            var endpoint = _config["Gemini:Endpoint"];
            var timeoutMs = _config.GetValue("Gemini:TimeoutMs", 8000);

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("[AI] Missing API Key for Gemini.");
                return ("REVIEW", "ai:missing_key", "fallback");
            }

            // Dùng model từ config, không hardcode nữa
            var modelName = _config["Gemini:Model"] ?? "gemini-1.5-flash";
            var url = string.IsNullOrWhiteSpace(endpoint)
                ? $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}"
                : endpoint.Replace("{MODEL}", modelName, StringComparison.OrdinalIgnoreCase).Replace("{API_KEY}", apiKey, StringComparison.OrdinalIgnoreCase);

            var debugUrl = url.Length > 20 ? url.Substring(0, url.Length - 10) + "**********" : url;
            _logger.LogInformation("[AI] Calling Gemini: {Url}", debugUrl);
            _logger.LogDebug("[AI] Prompt: {Prompt}", BuildCompactPrompt(message));

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
                if (resp.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("[AI] Gemini Quota Exceeded (429).");
                    return ("FAIL", "quota_exceeded", "gemini");
                }

                // Với các lỗi khác, dùng Internal Fallback
                return InternalFallbackScan(message);
            }

            var body = await resp.Content.ReadAsStringAsync(cts.Token);
            _logger.LogInformation("[AI] Gemini Raw Response: {Body}", body);
            
            var text = ExtractText(body);
            _logger.LogInformation("[AI] Gemini Extracted Text: '{Text}'", text);
            
            var label = ParseLabel(text);
            return (label, "ai:gemini", "gemini");
        }

        private async Task<(string Label, string Reason, string Source)> ModerateWithAiAsync(Guid messageId, string message, Guid eventId)
        {
            _logger.LogInformation("[AI-FLOW] Starting moderation for message {MessageId}", messageId);

            // Bước 1: Ưu tiên Bộ lọc Nội bộ (Internal Filter)
            var internalResult = InternalFallbackScan(message);
            if (internalResult.Label == "BLOCK")
            {
                _logger.LogInformation("[AI-FLOW] Internal Filter BLOCKED message {MessageId}", messageId);
                return internalResult;
            }

            // Bước 2: Nếu bộ lọc nội bộ cho qua, mới dùng AI (Groq)
            _logger.LogInformation("[AI-FLOW] Calling Groq for deeper analysis of message {MessageId}", messageId);
            var groq = await CallGroqAsync(message);
            
            return groq;
        }

        private async Task<(string Label, string Reason, string Source)> CallGroqAsync(string message)
        {
            var apiKey = _config["Groq:ApiKey"];
            var model = _config["Groq:Model"] ?? "llama-3.3-70b-versatile";
            var url = _config["Groq:Endpoint"] ?? "https://api.groq.com/openai/v1/chat/completions";
            var timeoutMs = _config.GetValue("Groq:TimeoutMs", 5000);

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("[AI] Missing API Key for Groq.");
                return InternalFallbackScan(message);
            }

            var prompt = BuildCompactPrompt(message);
            var payload = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.0,
                max_tokens = 20
            };

            var json = JsonSerializer.Serialize(payload);
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            req.Headers.Add("User-Agent", "Linkie-Moderator/1.0");

            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
            var client = _httpClientFactory.CreateClient("Groq");

            try 
            {
                using var resp = await client.SendAsync(req, cts.Token);
                if (!resp.IsSuccessStatusCode)
                {
                    var error = await resp.Content.ReadAsStringAsync(cts.Token);
                    _logger.LogError("[AI] Groq API Error ({Status}): {Body}", resp.StatusCode, error);
                    return InternalFallbackScan(message);
                }

                var body = await resp.Content.ReadAsStringAsync(cts.Token);
                using var doc = JsonDocument.Parse(body);
                var text = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                
                _logger.LogInformation("[AI] Groq Result: '{Text}'", text);
                var label = ParseLabel(text ?? "");
                return (label, "ai:groq", "groq");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AI] Groq Call Failed");
                return InternalFallbackScan(message);
            }
        }

        private (string Label, string Reason, string Source) InternalFallbackScan(string message)
        {
            var lower = message.ToLowerInvariant();
            
            // Comprehensive internal patterns for common insults and teencode
            var toxicPatterns = new[] 
            { 
                "óc", "lợn", "heo", "chó", "cc", "cl", "dm", "dmm", "cmm", "bùi", "cac", "cak", 
                "vcl", "vkl", "vl", "ngu", "đĩ", "lồn", "cặc", "cút", "mẹ mày", "con chó", "đẻ",
                "thằng", "mẹ", "dở", "tệ", "kém", "vô dụng", "nát", "rác", "đấm vào tai", "ngáo",
                "fuck", "shit", "bitch", "asshole", "dick", "pussy", "hỗn độn", "thất vọng", "kém", "súc vật",
                "chán", "không hay", "tệ hại", "xấu", "hát dở"
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
            return "BẠN LÀ MỘT CHUYÊN GIA KIỂM DUYỆT NỘI DUNG TIẾNG VIỆT NGHIÊM KHẮC.\n" +
                   "NHIỆM VỤ: Kiểm duyệt lời chúc cho sự kiện công cộng. Duy trì không khí tích cực.\n" +
                   "QUY TẮC:\n" +
                "1. CẤM (BLOCK): Các từ chửi bới, tục tĩu (cặc, lồn, đm, vcl...), các từ sỉ nhục động vật (súc vật, chó, lợn, bò...), mỉa mai tiêu cực hoặc chê bai (hát dở, hát chả hay, tổ chức tệ, mớ hỗn độn, thất vọng, kém, ngu, dốt, xấu, tệ...). BẤT KỲ LỜI CHÊ BAI NÀO VỀ CA SĨ, SỰ KIỆN HOẶC BAN TỔ CHỨC ĐỀU PHẢI BLOCK.\n" +
                "2. CẢNH BÁO (WARNING): Teencode khó hiểu, câu hỏi không liên quan, nội dung không có ý nghĩa rõ ràng.\n" +
                "3. CHO PHÉP (ALLOW): Chỉ những lời chúc thật sự tốt đẹp, tích cực, lịch sự mang tính cổ vũ.\n" +
                "LƯU Ý: Nếu có bất kỳ dấu hiệu tiêu cực, chê bai hoặc thiếu tôn trọng nào, hãy BLOCK ngay lập tức. Không được nương tay.\n" +
                   "Nội dung cần kiểm tra: \"" + message + "\"\n" +
                   "CHỈ TRẢ LỜI DUY NHẤT 1 TỪ (ALLOW, WARNING, hoặc BLOCK):";
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
            
            // Prioritize BLOCK
            if (t.Contains("BLOCK") || t.Contains("CẤM") || t.Contains("TỪ CHỐI")) return "BLOCK";
            
            // Then WARNING
            if (t.Contains("WARNING") || t.Contains("CẢNH BÁO")) return "WARNING";
            
            // Only ALLOW if explicitly found and no BLOCK/WARNING signs
            if (t.Contains("ALLOW") || t.Contains("CHO PHÉP")) return "ALLOW";

            // If the model refuses to answer or returns something else, be safe and BLOCK/WARNING
            return "WARNING";
        }
    }
}
