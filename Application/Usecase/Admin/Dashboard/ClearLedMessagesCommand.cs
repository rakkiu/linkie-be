using Domain.Interface;
using Application.Interfaces;
using Application.Model.Admin;
using Application.Model.WishwallAi;
using MediatR;

namespace Application.Usecase.Admin.Dashboard
{
    public record ClearLedMessagesCommand(Guid EventId) : IRequest<bool>;

    public class ClearLedMessagesHandler : IRequestHandler<ClearLedMessagesCommand, bool>
    {
        private readonly IAdminRepository _repo;

        public ClearLedMessagesHandler(IAdminRepository repo) => _repo = repo;

        public async Task<bool> Handle(ClearLedMessagesCommand request, CancellationToken cancellationToken)
        {
            await _repo.ClearLedMessagesAsync(request.EventId, cancellationToken);
            return true;
        }
    }
}
