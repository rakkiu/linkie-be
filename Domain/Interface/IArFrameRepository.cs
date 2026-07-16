using Domain.Entity;

namespace Domain.Interface
{
    public interface IArFrameRepository
    {
        Task<List<ArFrame>> GetActiveByEventIdAsync(Guid eventId, CancellationToken ct = default);
        Task<List<ArFrame>> GetAllByEventIdAsync(Guid eventId, CancellationToken ct = default);
        Task<ArFrame?> GetByIdAsync(Guid frameId, CancellationToken ct = default);
        Task AddAsync(ArFrame frame, CancellationToken ct = default);
        Task DeleteAsync(ArFrame frame, CancellationToken ct = default);
        Task AddUsageAsync(FrameUsage usage, CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
