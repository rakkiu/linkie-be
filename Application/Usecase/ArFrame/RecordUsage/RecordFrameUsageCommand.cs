using MediatR;

namespace Application.Usecase.ArFrame.RecordUsage
{
    public record RecordFrameUsageCommand(Guid EventId, Guid FrameId, Guid UserId) : IRequest<bool>;
}
