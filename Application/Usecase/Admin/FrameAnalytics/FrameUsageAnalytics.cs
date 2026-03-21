using Domain.Interface;
using Application.Interfaces;
using Application.Model.Admin;
using Application.Model.WishwallAi;
using MediatR;

namespace Application.Usecase.Admin.FrameAnalytics
{
    public class FrameStatDto
    {
        public Guid FrameId { get; set; }
        public string FrameName { get; set; } = string.Empty;
        public int Usage { get; set; }
    }

    public class FrameUsageAnalyticsDto
    {
        public int TotalPhotos { get; set; }
        public int ActiveFrames { get; set; }
        public List<FrameStatDto> Frames { get; set; } = new();
        public FrameStatDto? MostUsedFrame { get; set; }
    }

    public record GetFrameUsageAnalyticsQuery(Guid EventId) : IRequest<FrameUsageAnalyticsDto>;

    public class GetFrameUsageAnalyticsHandler : IRequestHandler<GetFrameUsageAnalyticsQuery, FrameUsageAnalyticsDto>
    {
        private readonly IAdminRepository _repo;

        public GetFrameUsageAnalyticsHandler(IAdminRepository repo) => _repo = repo;

        public async Task<FrameUsageAnalyticsDto> Handle(GetFrameUsageAnalyticsQuery request, CancellationToken cancellationToken)
        {
            var totalPhotos = await _repo.GetFrameUsageCountAsync(request.EventId, cancellationToken);
            var activeFrames = await _repo.GetActiveFrameCountAsync(request.EventId, cancellationToken);
            var frameStats = await _repo.GetFrameStatsAsync(request.EventId, cancellationToken);

            var frameDtos = frameStats.Select(f => new FrameStatDto
            {
                FrameId = f.FrameId,
                FrameName = f.FrameName,
                Usage = f.Usage
            }).ToList();

            return new FrameUsageAnalyticsDto
            {
                TotalPhotos = totalPhotos,
                ActiveFrames = activeFrames,
                Frames = frameDtos,
                MostUsedFrame = frameDtos.FirstOrDefault()
            };
        }
    }
}
