using Domain.Entity;
using Domain.Enums;
using Domain.Interface;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _db;

        public AdminRepository(ApplicationDbContext db) => _db = db;

        public Task<int> GetParticipantCountAsync(Guid eventId, CancellationToken ct = default)
            => _db.EventParticipants.CountAsync(p => p.EventId == eventId, ct);

        public Task<int> GetMessageCountAsync(Guid eventId, CancellationToken ct = default)
            => _db.WishwallMessages.CountAsync(m => m.EventId == eventId, ct);

        public Task<int> GetFrameUsageCountAsync(Guid eventId, CancellationToken ct = default)
            => _db.FrameUsages.CountAsync(f => f.EventId == eventId, ct);

        public async Task<Dictionary<WishwallSentiment, int>> GetSentimentSummaryAsync(Guid eventId, CancellationToken ct = default)
        {
            var rows = await _db.WishwallMessages
                .Where(m => m.EventId == eventId)
                .GroupBy(m => m.Sentiment)
                .Select(g => new { Sentiment = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            return rows.ToDictionary(x => x.Sentiment, x => x.Count);
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
                .Where(m => m.EventId == eventId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

        public Task<int> GetTotalWishwallCountAsync(Guid eventId, CancellationToken ct = default)
            => _db.WishwallMessages.CountAsync(m => m.EventId == eventId, ct);

        public Task<bool> PingAsync(CancellationToken ct = default)
            => _db.Database.CanConnectAsync(ct);
    }
}
