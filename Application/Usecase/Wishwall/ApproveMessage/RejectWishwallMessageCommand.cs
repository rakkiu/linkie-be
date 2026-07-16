using MediatR;

namespace Application.Usecase.Wishwall.ApproveMessage
{
    public record RejectWishwallMessageCommand(Guid MessageId) : IRequest<bool>;
}
