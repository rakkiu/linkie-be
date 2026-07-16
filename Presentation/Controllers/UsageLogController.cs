using Domain.Enums;
using Domain.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Presentation.Common;
using Infrastructure.Identity;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/admin/usage")]
    [Authorize(Roles = "Admin")]
    public class UsageLogController : ControllerBase
    {
        private readonly IUsageLogRepository _repo;
        private readonly ApplicationDbContext _dbContext;

        public UsageLogController(IUsageLogRepository repo, ApplicationDbContext dbContext)
        {
            _repo = repo;
            _dbContext = dbContext;
        }

        [HttpGet("kpi")]
        public async Task<IActionResult> GetKpi([FromQuery] DateTime? start, [FromQuery] DateTime? end, CancellationToken ct)
        {
            var actualStart = start ?? DateTime.UtcNow.AddMonths(-1);
            var actualEnd = end ?? DateTime.UtcNow;

            var activeBusinesses = await _repo.CountDistinctBusinessesAsync(actualStart, actualEnd, ct);
            var activeStaff = await _repo.CountDistinctStaffAsync(actualStart, actualEnd, ct);

            // Retroactive data from business tables as supplementary evidence
            var totalOrganizers = await _dbContext.Users
                .Where(u => u.Role == UserRole.Organizer)
                .CountAsync(ct);

            var totalStaffAccounts = await _dbContext.Users
                .Where(u => u.Role == UserRole.Staff)
                .CountAsync(ct);

            var organizersWithEvents = await _dbContext.Users
                .Where(u => u.Role == UserRole.Organizer && u.ManagedEventId != null)
                .CountAsync(ct);

            var staffWithActivity = await _dbContext.EventParticipants
                .Where(ep => ep.User.Role == UserRole.Staff)
                .Select(ep => ep.UserId)
                .Distinct()
                .CountAsync(ct);

            return Ok(new ApiResponse<object>
            {
                StatusCode = 200,
                Message = "KPI usage summary retrieved.",
                Data = new
                {
                    period = new { start = actualStart, end = actualEnd },
                    usageLogBased = new
                    {
                        activeBusinesses,
                        activeStaff
                    },
                    retroactiveEvidence = new
                    {
                        totalOrganizerAccounts = totalOrganizers,
                        organizersWithEvents,
                        totalStaffAccounts,
                        staffWithEventParticipation = staffWithActivity
                    }
                },
                ResponsedAt = DateTime.UtcNow
            });
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentUsage([FromQuery] int limit = 50, CancellationToken ct = default)
        {
            var logs = await _repo.GetRecentUsageAsync(limit, ct);
            return Ok(new ApiResponse<object>
            {
                StatusCode = 200,
                Message = "Recent usage logs retrieved.",
                Data = logs,
                ResponsedAt = DateTime.UtcNow
            });
        }
    }
}
