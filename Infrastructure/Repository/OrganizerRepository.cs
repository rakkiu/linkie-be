using Domain.Entity;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class OrganizerRepository : IOrganizerRepository
    {
        private readonly ApplicationDbContext _db;

        public OrganizerRepository(ApplicationDbContext db) => _db = db;

        public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)
        {
            var normalizedUsername = username.ToLower().Trim();
            return await _db.Users.AnyAsync(u => u.Username == normalizedUsername, ct);
        }

        public async Task<User?> GetOrganizerByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Users
                .Include(u => u.ManagedEvent)
                .FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Organizer, ct);
        }

        public async Task<List<User>> GetAllOrganizersAsync(CancellationToken ct = default)
        {
            return await _db.Users
                .Include(u => u.ManagedEvent)
                .Where(u => u.Role == UserRole.Organizer)
                .OrderBy(u => u.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<Event?> GetEventByIdAsync(Guid eventId, CancellationToken ct = default)
        {
            return await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct);
        }

        public async Task DeleteOrganizerAsync(User organizer, CancellationToken ct = default)
        {
            _db.Users.Remove(organizer);
            await _db.SaveChangesAsync(ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}
