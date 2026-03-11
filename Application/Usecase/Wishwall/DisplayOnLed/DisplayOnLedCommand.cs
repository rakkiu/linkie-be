using MediatR;

namespace Application.Usecase.Wishwall.DisplayOnLed
{
    public record DisplayOnLedCommand(Guid EventId, Guid MessageId) : IRequest<bool>;
}
