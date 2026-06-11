namespace Application.Usecase.EventManagement.ToggleTicketVerification
{
    public class ToggleTicketVerificationResponseDto
    {
        public Guid EventId { get; set; }
        public bool RequiresTicket { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
