using Domain.Entity;
using Domain.Enums;
using Domain.Interface;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class UsageLogRepository : IUsageLogRepository
    {
        private readonly ApplicationDbContext _context;

        public UsageLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UsageLog log, CancellationToken ct = default)
        {
            await _context.UsageLogs.AddAsync(log, ct);
        }

        public async Task AddRangeAsync(IEnumerable<UsageLog> logs, CancellationToken ct = default)
        {
            await _context.UsageLogs.AddRangeAsync(logs, ct);
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await _context.SaveChangesAsync(ct);
        }

        public async Task<int> CountDistinctBusinessesAsync(DateTime? start, DateTime? end, CancellationToken ct = default)
        {
            var query = _context.UsageLogs
                .Where(ul => ul.User.Role == UserRole.Organizer);

            if (start.HasValue)
                query = query.Where(ul => ul.CreatedAt >= start.Value);
            if (end.HasValue)
                query = query.Where(ul => ul.CreatedAt <= end.Value);

            return await query.Select(ul => ul.UserId).Distinct().CountAsync(ct);
        }

        public async Task<int> CountDistinctStaffAsync(DateTime? start, DateTime? end, CancellationToken ct = default)
        {
            var query = _context.UsageLogs
                .Where(ul => ul.User.Role == UserRole.Staff);

            if (start.HasValue)
                query = query.Where(ul => ul.CreatedAt >= start.Value);
            if (end.HasValue)
                query = query.Where(ul => ul.CreatedAt <= end.Value);

            return await query.Select(ul => ul.UserId).Distinct().CountAsync(ct);
        }

        public async Task<List<(Guid UserId, string Action, DateTime CreatedAt)>> GetRecentUsageAsync(int limit, CancellationToken ct = default)
        {
            return await _context.UsageLogs
                .OrderByDescending(ul => ul.CreatedAt)
                .Take(limit)
                .Select(ul => new ValueTuple<Guid, string, DateTime>(ul.UserId, ul.Action, ul.CreatedAt))
                .ToListAsync(ct);
        }
    }
}
