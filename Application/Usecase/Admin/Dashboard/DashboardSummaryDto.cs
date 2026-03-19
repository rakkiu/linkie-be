using Domain.Enums;
using Application.Model.WishwallAi;

namespace Application.Usecase.Admin.Dashboard
{
    public class DashboardSummaryDto
    {
        public int TotalParticipants { get; set; }
        public int TotalPhotos { get; set; }
        public int TotalPhotographers { get; set; }
        public int ActiveFramesCount { get; set; }
        public List<FrameStatsDto> FrameUsageStats { get; set; } = new();
        public Dictionary<WishwallSentiment, int> SentimentSummary { get; set; } = new();
        public List<LiveMessageDto> RecentLiveMessages { get; set; } = new();
        public WishwallAiSummaryDto AiSummary { get; set; } = new();
    }

    public class FrameStatsDto
    {
        public string FrameName { get; set; } = string.Empty;
        public int UsageCount { get; set; }
    }

    public class LiveMessageDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public WishwallSentiment Sentiment { get; set; }
    }
}
