using Domain.Entity;
using Domain.Interface;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class WishwallRepository : IWishwallRepository
    {
        private readonly ApplicationDbContext _db;

        public WishwallRepository(ApplicationDbContext db) => _db = db;

        // Public wishwall: only approved, visible messages
        public async Task<List<WishwallMessage>> GetByEventIdAsync(Guid eventId, CancellationToken ct = default)
            => await _db.WishwallMessages
                .Include(m => m.User)
                .Where(m => m.EventId == eventId && m.IsApproved && !m.IsHidden)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync(ct);

        // Staff view: pending (not approved, not hidden)
        public async Task<List<WishwallMessage>> GetPendingByEventIdAsync(Guid eventId, CancellationToken ct = default)
            => await _db.WishwallMessages
                .Include(m => m.User)
                .Where(m => m.EventId == eventId && !m.IsApproved && !m.IsHidden)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync(ct);

        public async Task<WishwallMessage?> GetByIdAsync(Guid messageId, CancellationToken ct = default)
            => await _db.WishwallMessages
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == messageId, ct);

        public async Task AddAsync(WishwallMessage message, CancellationToken ct = default)
            => await _db.WishwallMessages.AddAsync(message, ct);

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
