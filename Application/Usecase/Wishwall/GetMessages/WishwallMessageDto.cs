namespace Application.Usecase.Wishwall.GetMessages
{
    public class WishwallMessageDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Sentiment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
