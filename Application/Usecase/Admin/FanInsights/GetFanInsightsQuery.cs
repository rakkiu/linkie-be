using MediatR;
using Domain.Interface;

namespace Application.Usecase.Admin.FanInsights
{
    public record GetFanInsightsQuery(Guid EventId) : IRequest<List<UserFanInsightDto>>;

    public class GetFanInsightsHandler : IRequestHandler<GetFanInsightsQuery, List<UserFanInsightDto>>
    {
        private readonly IAdminRepository _adminRepository;

        public GetFanInsightsHandler(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public Task<List<UserFanInsightDto>> Handle(GetFanInsightsQuery request, CancellationToken cancellationToken)
        {
            return _adminRepository.GetFanInsightsAsync(request.EventId, cancellationToken);
        }
    }
}
