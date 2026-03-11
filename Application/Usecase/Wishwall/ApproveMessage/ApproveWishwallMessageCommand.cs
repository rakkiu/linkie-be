using MediatR;

namespace Application.Usecase.Wishwall.ApproveMessage
{
    public record ApproveWishwallMessageCommand(Guid MessageId) : IRequest<bool>;
}
