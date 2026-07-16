using Domain.Enums;
using Domain.Interface;
using Application.Interfaces;
using Application.Model.Admin;
using Application.Model.WishwallAi;
using MediatR;

namespace Application.Usecase.Admin.Sentiment
{
    public class SentimentDataPointDto
    {
        public string Time { get; set; } = string.Empty;
        public int Positive { get; set; }
        public int Neutral { get; set; }
        public int Negative { get; set; }
    }

    public record GetSentimentAnalyticsQuery(Guid EventId, string Interval = "minute") : IRequest<List<SentimentDataPointDto>>;

    public class GetSentimentAnalyticsHandler : IRequestHandler<GetSentimentAnalyticsQuery, List<SentimentDataPointDto>>
    {
        private readonly IAdminRepository _repo;

        public GetSentimentAnalyticsHandler(IAdminRepository repo) => _repo = repo;

        public async Task<List<SentimentDataPointDto>> Handle(GetSentimentAnalyticsQuery request, CancellationToken cancellationToken)
        {
            var messages = await _repo.GetMessagesWithCreatedAtAsync(request.EventId, cancellationToken);

            var grouped = messages
                .GroupBy(m => request.Interval == "hour"
                    ? m.CreatedAt.ToString("HH:00")
                    : m.CreatedAt.ToString("HH:mm"))
                .Select(g => new SentimentDataPointDto
                {
                    Time = g.Key,
                    Positive = g.Count(m => m.Sentiment == WishwallSentiment.Positive),
                    Neutral = g.Count(m => m.Sentiment == WishwallSentiment.Neutral),
                    Negative = g.Count(m => m.Sentiment == WishwallSentiment.Negative)
                })
                .OrderBy(x => x.Time)
                .ToList();

            return grouped;
        }
    }
}
