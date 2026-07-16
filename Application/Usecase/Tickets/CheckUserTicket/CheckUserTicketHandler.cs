using Domain.Interface;
using MediatR;

namespace Application.Usecase.Tickets.CheckUserTicket
{
    public class CheckUserTicketHandler : IRequestHandler<CheckUserTicketQuery, CheckUserTicketResponse>
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IEventRepository _eventRepository;

        public CheckUserTicketHandler(ITicketRepository ticketRepository, IEventRepository eventRepository)
        {
            _ticketRepository = ticketRepository;
            _eventRepository = eventRepository;
        }

        public async Task<CheckUserTicketResponse> Handle(CheckUserTicketQuery request, CancellationToken cancellationToken)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken);
            if (eventEntity == null)
            {
                return new CheckUserTicketResponse
                {
                    HasValidTicket = false,
                    Message = "Event not found"
                };
            }

            if (!eventEntity.RequiresTicket)
            {
                return new CheckUserTicketResponse
                {
                    HasValidTicket = true,
                    EventId = request.EventId,
                    RequiresTicket = false
                };
            }

            var ticket = await _ticketRepository.GetByUserAndEventAsync(request.UserId, request.EventId, cancellationToken);

            if (ticket == null)
            {
                return new CheckUserTicketResponse
                {
                    HasValidTicket = false,
                    Message = "You don't have a valid ticket for this event",
                    RequiresTicket = true
                };
            }

            return new CheckUserTicketResponse
            {
                HasValidTicket = true,
                TicketCode = ticket.TicketCode,
                TicketStatus = ticket.Status.ToString(),
                EventId = request.EventId,
                RequiresTicket = true
            };
        }
    }
}
