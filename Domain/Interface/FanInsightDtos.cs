using Domain.Enums;

namespace Domain.Interface
{
    public class UserFanInsightDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalPhotos { get; set; }
        public int TotalMessages { get; set; }
        public string UsedFrames { get; set; } = string.Empty; // e.g. "Frame 1 (8), Frame 3 (4)"
        public float EngagementScore { get; set; }
    }

    public class FanProfileDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<FramePreferenceDto> FramePreferences { get; set; } = new();
        public List<FanWishwallMessageDto> RecentMessages { get; set; } = new();
    }

    public class FramePreferenceDto
    {
        public Guid FrameId { get; set; }
        public string FrameName { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public double Percentage { get; set; }
    }

    public class FanWishwallMessageDto
    {
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public WishwallSentiment Sentiment { get; set; }
    }
}
