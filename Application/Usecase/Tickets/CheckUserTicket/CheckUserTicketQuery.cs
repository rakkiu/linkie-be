using MediatR;

namespace Application.Usecase.Tickets.CheckUserTicket
{
    public class CheckUserTicketQuery : IRequest<CheckUserTicketResponse>
    {
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
    }

    public class CheckUserTicketResponse
    {
        public bool HasValidTicket { get; set; }
        public string? TicketCode { get; set; }
        public string? TicketStatus { get; set; }
        public string? Message { get; set; }
        public Guid EventId { get; set; }
        public bool RequiresTicket { get; set; }
    }
}
