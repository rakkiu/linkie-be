using Domain.Entity;
using Domain.Enums;
using Domain.Interface;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext _db;

        public TicketRepository(ApplicationDbContext db) => _db = db;

        public async Task<Ticket?> GetByUserAndEventAsync(Guid userId, Guid eventId, CancellationToken ct = default)
            => await _db.Tickets
                .Where(t => t.UserId == userId && t.EventId == eventId && t.Status == TicketStatus.ACTIVE)
                .FirstOrDefaultAsync(ct);

        public async Task<Ticket?> GetByCodeAsync(string ticketCode, Guid eventId, CancellationToken ct = default)
            => await _db.Tickets
                .Where(t => t.TicketCode == ticketCode && t.EventId == eventId)
                .FirstOrDefaultAsync(ct);

        public async Task<bool> HasValidTicketAsync(Guid userId, Guid eventId, CancellationToken ct = default)
            => await _db.Tickets
                .AnyAsync(t => t.UserId == userId && t.EventId == eventId && t.Status == TicketStatus.ACTIVE, ct);

        public async Task AddRangeAsync(List<Ticket> tickets, CancellationToken ct = default)
        {
            await _db.Tickets.AddRangeAsync(tickets, ct);
        }

        public async Task<(List<Ticket> Tickets, int TotalCount)> GetPagedByEventAsync(
            Guid eventId, int page, int pageSize, string? statusFilter, CancellationToken ct = default)
        {
            var query = _db.Tickets
                .Include(t => t.User)
                .Where(t => t.EventId == eventId);

            if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<TicketStatus>(statusFilter, out var status))
            {
                query = query.Where(t => t.Status == status);
            }

            var totalCount = await query.CountAsync(ct);

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (tickets, totalCount);
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _db.SaveChangesAsync(ct);
    }
}
