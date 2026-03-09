using Domain.Enums;
using Domain.Interface;
using MediatR;

namespace Application.Usecase.Admin.Dashboard
{
    public class GetEventDashboardHandler : IRequestHandler<GetEventDashboardQuery, EventDashboardDto>
    {
        private readonly IAdminRepository _repo;

        public GetEventDashboardHandler(IAdminRepository repo) => _repo = repo;

        public async Task<EventDashboardDto> Handle(GetEventDashboardQuery request, CancellationToken cancellationToken)
        {
            // Sequential queries — EF Core DbContext is not thread-safe
            var totalParticipants = await _repo.GetParticipantCountAsync(request.EventId, cancellationToken);
            var totalMessages = await _repo.GetMessageCountAsync(request.EventId, cancellationToken);
            var totalPhotos = await _repo.GetFrameUsageCountAsync(request.EventId, cancellationToken);
            var sentimentMap = await _repo.GetSentimentSummaryAsync(request.EventId, cancellationToken);
            var topFrame = await _repo.GetTopFrameAsync(request.EventId, cancellationToken);

            return new EventDashboardDto
            {
                EventId = request.EventId,
                TotalParticipants = totalParticipants,
                TotalMessages = totalMessages,
                TotalPhotos = totalPhotos,
                SentimentSummary = new SentimentSummaryDto
                {
                    Positive = sentimentMap.GetValueOrDefault(WishwallSentiment.Positive),
                    Neutral = sentimentMap.GetValueOrDefault(WishwallSentiment.Neutral),
                    Negative = sentimentMap.GetValueOrDefault(WishwallSentiment.Negative)
                },
                TopFrame = topFrame.HasValue ? new TopFrameDto
                {
                    FrameId = topFrame.Value.FrameId,
                    FrameName = topFrame.Value.FrameName,
                    Usage = topFrame.Value.Usage
                } : null
            };
        }
    }
}
