namespace Application.Usecase.Admin.Dashboard
{
    public class EventDashboardDto
    {
        public Guid EventId { get; set; }
        public int TotalParticipants { get; set; }
        public int TotalMessages { get; set; }
        public int TotalPhotos { get; set; }
        public SentimentSummaryDto SentimentSummary { get; set; } = new();
        public TopFrameDto? TopFrame { get; set; }
    }

    public class SentimentSummaryDto
    {
        public int Positive { get; set; }
        public int Neutral { get; set; }
        public int Negative { get; set; }
    }

    public class TopFrameDto
    {
        public Guid FrameId { get; set; }
        public string FrameName { get; set; } = string.Empty;
        public int Usage { get; set; }
    }
}
