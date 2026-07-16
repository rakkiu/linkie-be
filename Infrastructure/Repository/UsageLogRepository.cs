using Domain.Entity;
using Domain.Enums;
using Domain.Interface;
using Infrastructure.Identity;
using Infrastructure.Security;
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

        public async Task<List<object>> GetRecentUsageAsync(int limit, CancellationToken ct = default)
        {
            var logs = await _context.UsageLogs
                .Include(ul => ul.User)
                .OrderByDescending(ul => ul.CreatedAt)
                .Take(limit)
                .ToListAsync(ct);
                
            var result = logs.Select(ul => new
            {
                ul.Id,
                ul.Action,
                ul.CreatedAt,
                ul.Metadata,
                ul.EntityId,
                ul.EntityType,
                User = ul.User == null ? null : new
                {
                    FirstName = EncryptionHelper.Decrypt(ul.User.Name),
                    LastName = "",
                    ul.User.Role,
                    AvatarUrl = (string?)null
                }
            }).Cast<object>().ToList();
            
            return result;
        }
    }
}
