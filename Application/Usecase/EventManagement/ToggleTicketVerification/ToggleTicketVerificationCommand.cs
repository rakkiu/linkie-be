using MediatR;

namespace Application.Usecase.EventManagement.ToggleTicketVerification
{
    public record ToggleTicketVerificationCommand(Guid EventId, bool RequiresTicket)
        : IRequest<ToggleTicketVerificationResponseDto>;
}
