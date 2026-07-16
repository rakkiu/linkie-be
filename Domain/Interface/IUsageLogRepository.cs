using Domain.Entity;

namespace Domain.Interface
{
    public interface IUsageLogRepository
    {
        Task AddAsync(UsageLog log, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<UsageLog> logs, CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        // KPI queries
        Task<int> CountDistinctBusinessesAsync(DateTime? start, DateTime? end, CancellationToken ct = default);
        Task<int> CountDistinctStaffAsync(DateTime? start, DateTime? end, CancellationToken ct = default);
        Task<List<object>> GetRecentUsageAsync(int limit, CancellationToken ct = default);
    }
}
