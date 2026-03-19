namespace Application.Model.WishwallAi
{
    public class WishwallAiLogDto
    {
        public Guid MessageId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public int DurationMs { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class WishwallAiSummaryDto
    {
        public int Total { get; set; }
        public int Allow { get; set; }
        public int Review { get; set; }
        public int Block { get; set; }
        public int Fallback { get; set; }
        public double AvgDurationMs { get; set; }
    }
}
