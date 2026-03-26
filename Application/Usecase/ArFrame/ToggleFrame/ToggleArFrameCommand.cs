using MediatR;

namespace Application.Usecase.ArFrame.ToggleFrame
{
    public record ToggleArFrameCommand(Guid FrameId) : IRequest<ToggleArFrameResult>;

    public record ToggleArFrameResult(Guid Id, bool IsActive);
}
