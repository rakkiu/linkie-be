using MediatR;

namespace Application.Usecase.Wishwall.DisplayOnLed
{
    /// <summary>
    /// Pushes an already-approved wishwall message to the LED screen group for the given event.
    /// </summary>
    public record DisplayOnLedCommand(Guid MessageId) : IRequest<bool>;
}
