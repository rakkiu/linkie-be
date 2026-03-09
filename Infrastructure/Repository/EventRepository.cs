using Domain.Entity;
using Domain.Enums;
using Domain.Interface;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _db;

        public EventRepository(ApplicationDbContext db) => _db = db;

        public async Task<List<Event>> GetByStatusAsync(EventStatus status, CancellationToken ct = default)
            => await _db.Events
                .Where(e => e.Status == status)
                .OrderBy(e => e.StartTime)
                .ToListAsync(ct);

        public async Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _db.Events.FirstOrDefaultAsync(e => e.Id == id, ct);
    }
}
