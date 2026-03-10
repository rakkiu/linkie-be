using MediatR;

namespace Application.Usecase.Wishwall.GetPendingMessages
{
    public record GetPendingWishwallMessagesQuery(Guid EventId) : IRequest<List<PendingWishwallMessageDto>>;
}
