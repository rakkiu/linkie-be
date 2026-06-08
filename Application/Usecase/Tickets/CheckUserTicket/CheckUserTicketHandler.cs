using Domain.Interface;
using MediatR;

namespace Application.Usecase.Tickets.CheckUserTicket
{
    public class CheckUserTicketHandler : IRequestHandler<CheckUserTicketQuery, CheckUserTicketResponse>
    {
        private readonly ITicketRepository _ticketRepository;

        public CheckUserTicketHandler(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public async Task<CheckUserTicketResponse> Handle(CheckUserTicketQuery request, CancellationToken cancellationToken)
        {
            var ticket = await _ticketRepository.GetByUserAndEventAsync(request.UserId, request.EventId, cancellationToken);

            if (ticket == null)
            {
                return new CheckUserTicketResponse
                {
                    HasValidTicket = false,
                    Message = "You don't have a valid ticket for this event"
                };
            }

            return new CheckUserTicketResponse
            {
                HasValidTicket = true,
                TicketCode = ticket.TicketCode,
                TicketStatus = ticket.Status.ToString()
            };
        }
    }
}
