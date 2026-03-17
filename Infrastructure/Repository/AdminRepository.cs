using Domain.Entity;
using Domain.Enums;
using Domain.Interface;
using Infrastructure.Identity;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IEncryptionService _encryptionService;

        public AdminRepository(ApplicationDbContext db, IEncryptionService encryptionService)
        {
            _db = db;
            _encryptionService = encryptionService;
        }

        public async Task<int> GetParticipantCountAsync(Guid eventId, CancellationToken ct = default)
        {
            var p1 = await _db.EventParticipants.Where(p => p.EventId == eventId).Select(p => p.UserId).ToListAsync(ct);
            var p2 = await _db.FrameUsages.Where(p => p.EventId == eventId).Select(p => p.UserId).ToListAsync(ct);
            var p3 = await _db.WishwallMessages.Where(p => p.EventId == eventId).Select(p => p.UserId).ToListAsync(ct);

            return p1.Union(p2).Union(p3).Distinct().Count();
        }

        public Task<int> GetMessageCountAsync(Guid eventId, CancellationToken ct = default)
            => _db.WishwallMessages.CountAsync(m => m.EventId == eventId, ct);

        public Task<int> GetFrameUsageCountAsync(Guid eventId, CancellationToken ct = default)
            => _db.FrameUsages.CountAsync(f => f.EventId == eventId, ct);

        public Task<int> GetPhotographerCountAsync(Guid eventId, CancellationToken ct = default)
            => _db.FrameUsages.Where(f => f.EventId == eventId).Select(f => f.UserId).Distinct().CountAsync(ct);

        public async Task<Dictionary<WishwallSentiment, int>> GetSentimentSummaryAsync(Guid eventId, CancellationToken ct = default)
        {
            // Lấy các tin nhắn đã duyệt theo sentiment thực tế
            var approvedRows = await _db.WishwallMessages
                .Where(m => m.EventId == eventId && m.IsApproved && !m.IsHidden)
                .GroupBy(m => m.Sentiment)
                .Select(g => new { Sentiment = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            // Đếm các tin nhắn bị ẩn (Rejected) hoặc có sentiment Negative
            var rejectedCount = await _db.WishwallMessages
                .CountAsync(m => m.EventId == eventId && m.IsHidden, ct);

            var result = approvedRows.ToDictionary(x => x.Sentiment, x => x.Count);

            if (!result.ContainsKey(WishwallSentiment.Negative))
                result[WishwallSentiment.Negative] = 0;
            
            result[WishwallSentiment.Negative] += rejectedCount;

            return result;
        }

        public async Task<(Guid FrameId, string FrameName, int Usage)?> GetTopFrameAsync(Guid eventId, CancellationToken ct = default)
        {
            var top = await _db.FrameUsages
                .Where(f => f.EventId == eventId)
                .GroupBy(f => new { f.FrameId, f.Frame.FrameName })
                .Select(g => new { g.Key.FrameId, g.Key.FrameName, Usage = g.Count() })
                .OrderByDescending(x => x.Usage)
                .FirstOrDefaultAsync(ct);

            if (top == null) return null;
            return (top.FrameId, top.FrameName, top.Usage);
        }

        public Task<List<WishwallMessage>> GetMessagesWithCreatedAtAsync(Guid eventId, CancellationToken ct = default)
            => _db.WishwallMessages
                .Where(m => m.EventId == eventId)
                .ToListAsync(ct);

        public async Task<List<(Guid FrameId, string FrameName, int Usage)>> GetFrameStatsAsync(Guid eventId, CancellationToken ct = default)
        {
            var stats = await _db.FrameUsages
                .Where(f => f.EventId == eventId)
                .GroupBy(f => new { f.FrameId, f.Frame.FrameName })
                .Select(g => new { g.Key.FrameId, g.Key.FrameName, Usage = g.Count() })
                .OrderByDescending(x => x.Usage)
                .ToListAsync(ct);

            return stats.Select(x => (x.FrameId, x.FrameName, x.Usage)).ToList();
        }

        public Task<int> GetActiveFrameCountAsync(Guid eventId, CancellationToken ct = default)
            => _db.ArFrames.CountAsync(f => f.EventId == eventId && f.IsActive, ct);

        public Task<List<WishwallMessage>> GetPagedWishwallMessagesAsync(Guid eventId, int page, int pageSize, CancellationToken ct = default)
            => _db.WishwallMessages
                .Include(m => m.User)
                .Where(m => m.EventId == eventId && m.IsApproved && !m.IsHidden)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

        public Task<int> GetTotalWishwallCountAsync(Guid eventId, CancellationToken ct = default)
            => _db.WishwallMessages.CountAsync(m => m.EventId == eventId, ct);

        public Task<bool> PingAsync(CancellationToken ct = default)
            => _db.Database.CanConnectAsync(ct);

        public async Task<List<UserFanInsightDto>> GetFanInsightsAsync(Guid eventId, CancellationToken ct = default)
        {
            // Lấy danh sách tất cả các User ID duy nhất đã tham gia hoặc tương tác với sự kiện
            var participantIds = await _db.EventParticipants
                .Where(ep => ep.EventId == eventId)
                .Select(ep => ep.UserId)
                .ToListAsync(ct);

            var frameUserIds = await _db.FrameUsages
                .Where(f => f.EventId == eventId)
                .Select(f => f.UserId)
                .ToListAsync(ct);

            var messageUserIds = await _db.WishwallMessages
                .Where(m => m.EventId == eventId)
                .Select(m => m.UserId)
                .ToListAsync(ct);

            var allUserIds = participantIds
                .Union(frameUserIds)
                .Union(messageUserIds)
                .Distinct()
                .ToList();

            var users = await _db.Users
                .Where(u => allUserIds.Contains(u.Id))
                .ToListAsync(ct);

            Console.WriteLine($"[DEBUG] FanInsights for Event {eventId}: Found {allUserIds.Count} unique IDs, {users.Count} users in DB.");

            var result = new List<UserFanInsightDto>();

            foreach (var user in users)
            {
                // Tính toán trực tiếp số lượng ảnh chụp (số lượt dùng frame) của user trong event này
                var frameUsages = await _db.FrameUsages
                    .Where(f => f.EventId == eventId && f.UserId == user.Id)
                    .GroupBy(f => f.Frame.FrameName)
                    .Select(g => new { Name = g.Key, Count = g.Count() })
                    .ToListAsync(ct);

                var framesString = string.Join(", ", frameUsages.Select(f => $"{f.Name} ({f.Count})"));
                var totalPhotos = frameUsages.Sum(f => f.Count);

                // Tính toán số lượng tin nhắn Wishwall của user trong event này
                var totalMessages = await _db.WishwallMessages
                    .CountAsync(m => m.EventId == eventId && m.UserId == user.Id, ct);

                // Lấy điểm tương tác từ bảng Stats nếu có, nếu không tính đơn giản (Photos * 2 + Messages)
                var stat = await _db.UserEventStats
                    .FirstOrDefaultAsync(s => s.EventId == eventId && s.UserId == user.Id, ct);
                
                var engagementScore = stat?.EngagementScore ?? (totalPhotos * 2 + totalMessages);

                result.Add(new UserFanInsightDto()
                {
                    UserId = user.Id,
                    Name = _encryptionService.Decrypt(user.Name),
                    Email = _encryptionService.DecryptDeterministic(user.Email),
                    TotalPhotos = totalPhotos,
                    TotalMessages = totalMessages,
                    UsedFrames = framesString,
                    EngagementScore = (float)engagementScore
                });
            }

            return result.OrderByDescending(r => r.EngagementScore).ToList();
        }

        public async Task<FanProfileDto?> GetFanProfileAsync(Guid eventId, Guid userId, CancellationToken ct = default)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == userId, ct);

            if (user == null) return null;

            var profile = new FanProfileDto()
            {
                UserId = userId,
                Name = _encryptionService.Decrypt(user.Name)
            };

            // Frame preferences
            var frameStats = await _db.FrameUsages
                .Where(f => f.EventId == eventId && f.UserId == userId)
                .GroupBy(f => new { f.FrameId, f.Frame.FrameName })
                .Select(g => new { g.Key.FrameId, g.Key.FrameName, Count = g.Count() })
                .ToListAsync(ct);

            int totalUsage = frameStats.Sum(x => x.Count);

            profile.FramePreferences = frameStats.Select(f => new FramePreferenceDto
            {
                FrameId = f.FrameId,
                FrameName = f.FrameName,
                UsageCount = f.Count,
                Percentage = totalUsage > 0 ? (double)f.Count / totalUsage * 100 : 0
            }).OrderByDescending(f => f.UsageCount).ToList();

            // Recent messages
            profile.RecentMessages = await _db.WishwallMessages
                .Where(m => m.EventId == eventId && m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new FanWishwallMessageDto
                {
                    Content = m.Message,
                    CreatedAt = m.CreatedAt,
                    Sentiment = m.Sentiment
                })
                .ToListAsync(ct);

            return profile;
        }

        public async Task ClearLedMessagesAsync(Guid eventId, CancellationToken ct = default)
        {
            var messages = await _db.WishwallMessages
                .Where(m => m.EventId == eventId && m.IsApproved && !m.IsDisplayedOnLed)
                .ToListAsync(ct);

            foreach (var m in messages)
            {
                m.IsDisplayedOnLed = true;
            }

            await _db.SaveChangesAsync(ct);
        }
    }
}
