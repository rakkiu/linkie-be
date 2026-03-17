using Domain.Interface;
using MediatR;

namespace Application.Usecase.Admin.Dashboard
{
    public record ClearLedMessagesCommand(Guid EventId) : IRequest;

    public class ClearLedMessagesHandler : IRequestHandler<ClearLedMessagesCommand>
    {
        private readonly IAdminRepository _repo;

        public ClearLedMessagesHandler(IAdminRepository repo) => _repo = repo;

        public async Task Handle(ClearLedMessagesCommand request, CancellationToken cancellationToken)
        {
            await _repo.ClearLedMessagesAsync(request.EventId, cancellationToken);
        }
    }
}
