using MediatR;

namespace Application.Usecase.Wishwall.GetMessages
{
    public record GetWishwallMessagesQuery(Guid EventId) : IRequest<List<WishwallMessageDto>>;
}
