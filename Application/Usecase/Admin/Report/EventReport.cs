using Domain.Interface;
using MediatR;
using System.Text.RegularExpressions;

namespace Application.Usecase.Admin.Report
{
    public class KeywordStatDto
    {
        public string Keyword { get; set; } = string.Empty;
        public int Frequency { get; set; }
    }

    public class HeatMapPointDto
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class FrameUsageItemDto
    {
        public Guid FrameId { get; set; }
        public string FrameName { get; set; } = string.Empty;
        public int Usage { get; set; }
    }

    public class FrameUsageReportDto
    {
        public int TotalPhotos { get; set; }
        public int ActiveFrames { get; set; }
        public List<FrameUsageItemDto> Frames { get; set; } = new();
    }

    public class WishwallReportDto
    {
        public int PositiveCount { get; set; }
        public int NegativeCount { get; set; }
        public int NeutralCount { get; set; }
        public double PositiveRate { get; set; }
        public double NegativeRate { get; set; }
        public List<KeywordStatDto> TopKeywords { get; set; } = new();
    }

    public class EventReportDto
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }

        public int TotalTraffic { get; set; }
        public int TotalEngagement { get; set; }
        public double ConversionRate { get; set; }
        public int AverageSessionSeconds { get; set; }

        public FrameUsageReportDto FrameUsage { get; set; } = new();
        public WishwallReportDto Wishwall { get; set; } = new();
        public List<HeatMapPointDto> HeatMap { get; set; } = new();
    }

    public record GetEventReportQuery(Guid EventId) : IRequest<EventReportDto>;

    public class GetEventReportHandler : IRequestHandler<GetEventReportQuery, EventReportDto>
    {
        private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "va", "và", "la", "là", "the", "and", "for", "with", "that", "this", "you", "your",
            "toi", "tôi", "minh", "mình", "ban", "bạn", "anh", "chi", "em", "cua", "của",
            "mot", "một", "nhung", "những", "nay", "này", "qua", "quá", "roi", "rồi",
            "hay", "nhe", "nha", "di", "đi", "duoc", "được", "rat", "rất", "lam", "làm"
        };

        private readonly IAdminRepository _adminRepo;
        private readonly IEventRepository _eventRepo;

        public GetEventReportHandler(IAdminRepository adminRepo, IEventRepository eventRepo)
        {
            _adminRepo = adminRepo;
            _eventRepo = eventRepo;
        }

        public async Task<EventReportDto> Handle(GetEventReportQuery request, CancellationToken cancellationToken)
        {
            var ev = await _eventRepo.GetByIdAsync(request.EventId, cancellationToken)
                     ?? throw new KeyNotFoundException("Event not found.");

            var participants = await _adminRepo.GetParticipantCountAsync(request.EventId, cancellationToken);
            var totalMessages = await _adminRepo.GetMessageCountAsync(request.EventId, cancellationToken);
            var totalPhotos = await _adminRepo.GetFrameUsageCountAsync(request.EventId, cancellationToken);
            var activeFrames = await _adminRepo.GetActiveFrameCountAsync(request.EventId, cancellationToken);
            var frameStats = await _adminRepo.GetFrameStatsAsync(request.EventId, cancellationToken);
            var messages = await _adminRepo.GetMessagesWithCreatedAtAsync(request.EventId, cancellationToken);

            var positiveCount = messages.Count(m => m.Sentiment == Domain.Enums.WishwallSentiment.Positive);
            var negativeCount = messages.Count(m => m.Sentiment == Domain.Enums.WishwallSentiment.Negative);
            var neutralCount = messages.Count(m => m.Sentiment == Domain.Enums.WishwallSentiment.Neutral);

            var totalTraffic = participants;
            var totalEngagement = totalMessages + totalPhotos;
            var conversionRate = totalTraffic == 0 ? 0 : Math.Round((double)totalEngagement * 100 / totalTraffic, 1);
            var averageSessionSeconds = totalTraffic == 0
                ? 0
                : Math.Clamp(120 + (totalMessages * 20 + totalPhotos * 15) / Math.Max(1, totalTraffic), 30, 3600);

            var posNegTotal = positiveCount + negativeCount;
            var positiveRate = posNegTotal == 0 ? 0 : Math.Round((double)positiveCount * 100 / posNegTotal, 1);
            var negativeRate = posNegTotal == 0 ? 0 : Math.Round((double)negativeCount * 100 / posNegTotal, 1);

            var heatMap = messages
                .GroupBy(m => new DateTime(m.CreatedAt.Year, m.CreatedAt.Month, m.CreatedAt.Day, m.CreatedAt.Hour, 0, 0))
                .OrderBy(g => g.Key)
                .Select(g => new HeatMapPointDto
                {
                    Label = g.Key.ToString("HH:mm"),
                    Value = g.Count()
                })
                .ToList();

            var topKeywords = ExtractTopKeywords(messages.Select(m => m.Message));

            return new EventReportDto
            {
                EventId = ev.Id,
                EventName = ev.Name,
                GeneratedAt = DateTime.UtcNow,
                TotalTraffic = totalTraffic,
                TotalEngagement = totalEngagement,
                ConversionRate = conversionRate,
                AverageSessionSeconds = averageSessionSeconds,
                FrameUsage = new FrameUsageReportDto
                {
                    TotalPhotos = totalPhotos,
                    ActiveFrames = activeFrames,
                    Frames = frameStats.Select(f => new FrameUsageItemDto
                    {
                        FrameId = f.FrameId,
                        FrameName = f.FrameName,
                        Usage = f.Usage
                    }).ToList()
                },
                Wishwall = new WishwallReportDto
                {
                    PositiveCount = positiveCount,
                    NegativeCount = negativeCount,
                    NeutralCount = neutralCount,
                    PositiveRate = positiveRate,
                    NegativeRate = negativeRate,
                    TopKeywords = topKeywords
                },
                HeatMap = heatMap
            };
        }

        private static List<KeywordStatDto> ExtractTopKeywords(IEnumerable<string> messages)
        {
            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var message in messages)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    continue;
                }

                var words = Regex.Split(message.ToLowerInvariant(), @"[^\p{L}\p{Nd}]+")
                    .Where(w => !string.IsNullOrWhiteSpace(w))
                    .Where(w => w.Length >= 3)
                    .Where(w => !StopWords.Contains(w));

                foreach (var word in words)
                {
                    counts[word] = counts.TryGetValue(word, out var value) ? value + 1 : 1;
                }
            }

            return counts
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => kv.Key)
                .Take(6)
                .Select(kv => new KeywordStatDto
                {
                    Keyword = kv.Key,
                    Frequency = kv.Value
                })
                .ToList();
        }
    }
}
