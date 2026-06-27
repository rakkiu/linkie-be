using Domain.Entity;

namespace Domain.Interfaces
{
    public interface IOrganizerRepository
    {
        Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);
        Task<User?> GetOrganizerByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<User>> GetAllOrganizersAsync(CancellationToken ct = default);
        Task<Event?> GetEventByIdAsync(Guid eventId, CancellationToken ct = default);
        Task DeleteOrganizerAsync(User organizer, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
