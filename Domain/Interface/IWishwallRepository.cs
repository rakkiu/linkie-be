using Domain.Entity;

namespace Domain.Interface
{
    public interface IWishwallRepository
    {
        // Returns only approved, non-hidden messages (public wishwall view)
        Task<List<WishwallMessage>> GetByEventIdAsync(Guid eventId, CancellationToken ct = default);
        // Returns pending (not yet approved, not hidden) messages (staff view)
        Task<List<WishwallMessage>> GetPendingByEventIdAsync(Guid eventId, CancellationToken ct = default);
        Task<WishwallMessage?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(WishwallMessage message, CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
