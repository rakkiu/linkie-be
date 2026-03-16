using Domain.Enums;
using MediatR;

namespace Application.Usecase.Wishwall.ApproveMessage
{
    public record ApproveWishwallMessageCommand(Guid MessageId, WishwallSentiment Sentiment) : IRequest<bool>;
}
