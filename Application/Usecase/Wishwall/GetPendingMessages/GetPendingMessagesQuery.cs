using MediatR;

namespace Application.Usecase.Wishwall.GetPendingMessages
{
    public record GetPendingMessagesQuery(Guid EventId) : IRequest<List<PendingMessageDto>>;
}
