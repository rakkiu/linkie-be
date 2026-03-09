using Domain.Entity;
using Domain.Enums;

namespace Domain.Interface
{
    public interface IEventRepository
    {
        Task<List<Event>> GetByStatusAsync(EventStatus status, CancellationToken ct = default);
        Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default);
    }
}
