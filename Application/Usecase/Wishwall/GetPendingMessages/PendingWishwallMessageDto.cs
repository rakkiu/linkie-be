namespace Application.Usecase.Wishwall.GetPendingMessages
{
    public class PendingWishwallMessageDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Sentiment { get; set; } = string.Empty;
        public string? AiLabel { get; set; }
        public string? AiReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
