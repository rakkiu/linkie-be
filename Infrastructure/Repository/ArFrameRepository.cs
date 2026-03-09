using Domain.Entity;
using Domain.Interface;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class ArFrameRepository : IArFrameRepository
    {
        private readonly ApplicationDbContext _db;

        public ArFrameRepository(ApplicationDbContext db) => _db = db;

        public async Task<List<ArFrame>> GetActiveByEventIdAsync(Guid eventId, CancellationToken ct = default)
            => await _db.ArFrames
                .Where(f => f.EventId == eventId && f.IsActive)
                .ToListAsync(ct);

        public async Task<ArFrame?> GetByIdAsync(Guid frameId, CancellationToken ct = default)
            => await _db.ArFrames.FirstOrDefaultAsync(f => f.Id == frameId, ct);

        public async Task AddUsageAsync(FrameUsage usage, CancellationToken ct = default)
            => await _db.FrameUsages.AddAsync(usage, ct);

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
