using Domain.Interface;
using MediatR;

namespace Application.Usecase.Tickets.GetEventTickets
{
    public class GetEventTicketsHandler : IRequestHandler<GetEventTicketsQuery, GetEventTicketsResponse>
    {
        private readonly ITicketRepository _ticketRepository;

        public GetEventTicketsHandler(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public async Task<GetEventTicketsResponse> Handle(GetEventTicketsQuery request, CancellationToken cancellationToken)
        {
            var (tickets, totalCount) = await _ticketRepository.GetPagedByEventAsync(
                request.EventId, request.Page, request.PageSize, request.Status, cancellationToken);

            return new GetEventTicketsResponse
            {
                EventId = request.EventId,
                TotalRecords = totalCount,
                Tickets = tickets.Select(t => new TicketDetailDto
                {
                    TicketId = t.TicketId,
                    TicketCode = t.TicketCode,
                    Email = t.Email,
                    UserId = t.UserId,
                    UserName = t.User?.Name,
                    Status = t.Status.ToString(),
                    AssignedAt = t.AssignedAt
                }).ToList()
            };
        }
    }
}
