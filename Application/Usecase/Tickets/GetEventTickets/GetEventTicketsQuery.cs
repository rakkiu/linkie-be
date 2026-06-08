using MediatR;

namespace Application.Usecase.Tickets.GetEventTickets
{
    public class GetEventTicketsQuery : IRequest<GetEventTicketsResponse>
    {
        public Guid EventId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Status { get; set; }
    }

    public class GetEventTicketsResponse
    {
        public Guid EventId { get; set; }
        public int TotalRecords { get; set; }
        public List<TicketDetailDto> Tickets { get; set; } = new();
    }

    public class TicketDetailDto
    {
        public Guid TicketId { get; set; }
        public string TicketCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? AssignedAt { get; set; }
    }
}
