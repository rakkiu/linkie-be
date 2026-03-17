using Domain.Interface;
using MediatR;

using Application.Interfaces;

namespace Application.Usecase.Admin.Dashboard
{
    public record GetDashboardSummaryQuery(Guid EventId) : IRequest<DashboardSummaryDto>;

    public class GetDashboardSummaryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
    {
        private readonly IAdminRepository _repo;
        private readonly IEncryptionService _encryptionService;

        public GetDashboardSummaryHandler(IAdminRepository repo, IEncryptionService encryptionService)
        {
            _repo = repo;
            _encryptionService = encryptionService;
        }

        public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            var eventId = request.EventId;

            var participants = await _repo.GetParticipantCountAsync(eventId, cancellationToken);
            var photos = await _repo.GetFrameUsageCountAsync(eventId, cancellationToken);
            var activeFrames = await _repo.GetActiveFrameCountAsync(eventId, cancellationToken);
            var photographers = await _repo.GetPhotographerCountAsync(eventId, cancellationToken);
            var sentiment = await _repo.GetSentimentSummaryAsync(eventId, cancellationToken);
            var frameStats = await _repo.GetFrameStatsAsync(eventId, cancellationToken);
            
            // Lấy 5 tin nhắn gần nhất đã duyệt (được giả định là OnLed)
            var recentMessages = await _repo.GetPagedWishwallMessagesAsync(eventId, 1, 50, cancellationToken);

            return new DashboardSummaryDto
            {
                TotalParticipants = participants,
                TotalPhotos = photos,
                TotalPhotographers = photographers,
                ActiveFramesCount = activeFrames,
                SentimentSummary = sentiment,
                FrameUsageStats = frameStats.Select(f => new FrameStatsDto
                {
                    FrameName = f.FrameName,
                    UsageCount = f.Usage
                }).ToList(),
                RecentLiveMessages = recentMessages.Select(m => new LiveMessageDto
                {
                    UserName = _encryptionService.Decrypt(m.User.Name),
                    Content = m.Message,
                    CreatedAt = m.CreatedAt,
                    Sentiment = m.Sentiment
                }).ToList()
            };
        }
    }
}
