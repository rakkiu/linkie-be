using MediatR;
using Domain.Interface;

namespace Application.Usecase.Admin.FanInsights
{
    public record GetFanProfileQuery(Guid EventId, Guid UserId) : IRequest<FanProfileDto?>;

    public class GetFanProfileHandler : IRequestHandler<GetFanProfileQuery, FanProfileDto?>
    {
        private readonly IAdminRepository _adminRepository;

        public GetFanProfileHandler(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public Task<FanProfileDto?> Handle(GetFanProfileQuery request, CancellationToken cancellationToken)
        {
            return _adminRepository.GetFanProfileAsync(request.EventId, request.UserId, cancellationToken);
        }
    }
}
