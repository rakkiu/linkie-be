namespace Application.Usecase.Wishwall.GetPendingMessages
{
    public class PendingMessageDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Sentiment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
