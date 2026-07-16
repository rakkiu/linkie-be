using Application.Interfaces;
using Application.Model.WishwallAi;
using MediatR;

namespace Application.Usecase.Admin.WishwallAi
{
    public record GetWishwallAiLogsQuery(Guid EventId, int Take = 200) : IRequest<List<WishwallAiLogDto>>;

    public class GetWishwallAiLogsHandler : IRequestHandler<GetWishwallAiLogsQuery, List<WishwallAiLogDto>>
    {
        private readonly IAdminRepository _repo;

        public GetWishwallAiLogsHandler(IAdminRepository repo)
        {
            _repo = repo;
        }

        public Task<List<WishwallAiLogDto>> Handle(GetWishwallAiLogsQuery request, CancellationToken cancellationToken)
            => _repo.GetWishwallAiLogsAsync(request.EventId, request.Take, cancellationToken);
    }
}
