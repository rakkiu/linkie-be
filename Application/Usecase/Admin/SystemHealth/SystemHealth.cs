using System.Diagnostics;
using Domain.Interface;
using Application.Interfaces;
using Application.Model.Admin;
using Application.Model.WishwallAi;
using MediatR;

namespace Application.Usecase.Admin.SystemHealth
{
    public class SystemHealthDto
    {
        public string Status { get; set; } = "healthy";
        public long WebLatencyMs { get; set; }
        public DateTime ServerTime { get; set; }
    }

    public record GetSystemHealthQuery : IRequest<SystemHealthDto>;

    public class GetSystemHealthHandler : IRequestHandler<GetSystemHealthQuery, SystemHealthDto>
    {
        private readonly IAdminRepository _repo;

        public GetSystemHealthHandler(IAdminRepository repo) => _repo = repo;

        public async Task<SystemHealthDto> Handle(GetSystemHealthQuery request, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            var canConnect = await _repo.PingAsync(cancellationToken);
            sw.Stop();

            return new SystemHealthDto
            {
                Status = canConnect ? "healthy" : "degraded",
                WebLatencyMs = sw.ElapsedMilliseconds,
                ServerTime = DateTime.UtcNow
            };
        }
    }
}
