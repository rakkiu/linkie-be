using MediatR;

namespace Application.Usecase.Tickets.ImportTickets
{
    public class ImportTicketsCommand : IRequest<ImportTicketsResponse>
    {
        public Guid EventId { get; set; }
        public Stream FileStream { get; set; } = null!;
    }

    public class ImportTicketsResponse
    {
        public bool Success { get; set; }
        public Guid EventId { get; set; }
        public int TotalRecords { get; set; }
        public int ImportedTickets { get; set; }
        public List<FailedRecord> FailedRecords { get; set; } = new();
        public DateTime ImportedAt { get; set; }
    }

    public class FailedRecord
    {
        public int RowNumber { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
