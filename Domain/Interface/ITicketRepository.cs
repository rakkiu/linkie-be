using Domain.Entity;

namespace Domain.Interface
{
    public interface ITicketRepository
    {
        Task<Ticket?> GetByUserAndEventAsync(Guid userId, Guid eventId, CancellationToken ct = default);
        Task<Ticket?> GetByCodeAsync(string ticketCode, Guid eventId, CancellationToken ct = default);
        Task<bool> HasValidTicketAsync(Guid userId, Guid eventId, CancellationToken ct = default);
        Task AddRangeAsync(List<Ticket> tickets, CancellationToken ct = default);
        Task<(List<Ticket> Tickets, int TotalCount)> GetPagedByEventAsync(Guid eventId, int page, int pageSize, string? statusFilter, CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
