namespace Domain.Entity
{
    public class WishwallAiLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid MessageId { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public int DurationMs { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
