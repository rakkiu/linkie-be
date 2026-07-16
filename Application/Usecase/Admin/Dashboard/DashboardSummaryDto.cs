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
        public int TotalShares { get; set; }
        public int TotalTimelapses { get; set; }
        public List<FrameStatsDto> FrameUsageStats { get; set; } = new();
        public Dictionary<WishwallSentiment, int> SentimentSummary { get; set; } = new();
        public List<LiveMessageDto> RecentLiveMessages { get; set; } = new();
        public WishwallAiSummaryDto AiSummary { get; set; } = new();
        
        // Rating Metrics
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public List<RecentFeedbackDto> RecentFeedbacks { get; set; } = new();
    }

    public class RecentFeedbackDto
    {
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
        public int StarRating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
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
