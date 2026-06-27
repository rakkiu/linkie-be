using Application.Interfaces;
using Application.Model.WishwallAi;
using MediatR;

namespace Application.Usecase.Admin.Dashboard
{
    public record GetDashboardSummaryQuery(Guid EventId) : IRequest<DashboardSummaryDto>;

    public class GetDashboardSummaryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
    {
        private readonly IAdminRepository _repo;
        private readonly IEncryptionService _encryptionService;
        private readonly Domain.Interface.IEventRatingRepository _ratingRepo;
        private readonly Domain.Interface.IEventRepository _eventRepo;

        public GetDashboardSummaryHandler(IAdminRepository repo, IEncryptionService encryptionService, Domain.Interface.IEventRatingRepository ratingRepo, Domain.Interface.IEventRepository eventRepo)
        {
            _repo = repo;
            _encryptionService = encryptionService;
            _ratingRepo = ratingRepo;
            _eventRepo = eventRepo;
        }

        public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            var eventId = request.EventId;
            var eventEntity = await _eventRepo.GetByIdAsync(eventId, cancellationToken);
            var participants = await _repo.GetParticipantCountAsync(eventId, cancellationToken);
            var photos = await _repo.GetFrameUsageCountAsync(eventId, cancellationToken);
            var activeFrames = await _repo.GetActiveFrameCountAsync(eventId, cancellationToken);
            var photographers = await _repo.GetPhotographerCountAsync(eventId, cancellationToken);
            var sentiment = await _repo.GetSentimentSummaryAsync(eventId, cancellationToken);
            var frameStats = await _repo.GetFrameStatsAsync(eventId, cancellationToken);
            var aiSummary = await _repo.GetWishwallAiSummaryAsync(eventId, cancellationToken);
            
            // Lấy 5 tin nhắn gần nhất đã duyệt (được giả định là OnLed)
            var recentMessages = await _repo.GetPagedWishwallMessagesAsync(eventId, 1, 50, cancellationToken);
            
            // Get Ratings
            var ratings = await _ratingRepo.GetRatingsByEventIdAsync(eventId, cancellationToken);
            var totalReviews = ratings.Count;
            var averageRating = totalReviews > 0 ? ratings.Average(r => r.StarRating) : 0;
            var ratingDistribution = Enumerable.Range(1, 5).ToDictionary(x => x, x => 0);
            foreach (var rating in ratings)
            {
                if (ratingDistribution.ContainsKey(rating.StarRating))
                    ratingDistribution[rating.StarRating]++;
            }

            return new DashboardSummaryDto
            {
                TotalParticipants = participants,
                TotalPhotos = photos,
                TotalPhotographers = photographers,
                ActiveFramesCount = activeFrames,
                TotalShares = eventEntity?.TotalShares ?? 0,
                TotalTimelapses = eventEntity?.TotalTimelapses ?? 0,
                SentimentSummary = sentiment,
                FrameUsageStats = frameStats.Select(f => new FrameStatsDto
                {
                    FrameName = f.FrameName,
                    UsageCount = f.Usage
                }).ToList(),
                AiSummary = aiSummary,
                RecentLiveMessages = recentMessages.Select(m => new LiveMessageDto
                {
                    UserName = _encryptionService.Decrypt(m.User.Name),
                    Content = m.Message,
                    CreatedAt = m.CreatedAt,
                    Sentiment = m.Sentiment
                }).ToList(),
                TotalReviews = totalReviews,
                AverageRating = Math.Round(averageRating, 1),
                RatingDistribution = ratingDistribution,
                RecentFeedbacks = ratings.OrderByDescending(r => r.CreatedAt).Take(5).Select(r => new RecentFeedbackDto
                {
                    AuthorName = r.User != null && !string.IsNullOrEmpty(r.User.Name) ? _encryptionService.Decrypt(r.User.Name) : "Khách",
                    AuthorEmail = r.User != null ? r.User.Email : string.Empty,
                    StarRating = r.StarRating,
                    Comment = $"Đánh giá {r.StarRating} sao",
                    CreatedAt = r.CreatedAt
                }).ToList()
            };
        }
    }
}
