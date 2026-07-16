using MediatR;

namespace Application.Usecase.Wishwall.ApproveMessage
{
    public record ApproveMessageCommand(Guid EventId, Guid MessageId) : IRequest<bool>;
}
