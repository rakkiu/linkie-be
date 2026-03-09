using MediatR;

namespace Application.Usecase.Wishwall.SendMessage
{
    public record SendWishwallMessageCommand(Guid EventId, Guid UserId, string Message) : IRequest<bool>;
}
