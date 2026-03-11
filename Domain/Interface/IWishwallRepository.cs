using Domain.Entity;

namespace Domain.Interface
{
    public interface IWishwallRepository
    {
        Task<List<WishwallMessage>> GetByEventIdAsync(Guid eventId, CancellationToken ct = default);
        Task<List<WishwallMessage>> GetPendingByEventIdAsync(Guid eventId, CancellationToken ct = default);
        Task<WishwallMessage?> GetByIdAsync(Guid messageId, CancellationToken ct = default);
        Task AddAsync(WishwallMessage message, CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
