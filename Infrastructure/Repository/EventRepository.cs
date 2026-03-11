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

        public async Task<List<Event>> GetAllAsync(CancellationToken ct = default)
            => await _db.Events
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync(ct);

        public async Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _db.Events.FirstOrDefaultAsync(e => e.Id == id, ct);

        public async Task AddAsync(Event @event, CancellationToken ct = default)
            => await _db.Events.AddAsync(@event, ct);

        public Task UpdateAsync(Event @event, CancellationToken ct = default)
        {
            _db.Events.Update(@event);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Event @event, CancellationToken ct = default)
        {
            _db.Events.Remove(@event);
            return Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _db.SaveChangesAsync(ct);
    }
}
