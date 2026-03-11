using MediatR;

namespace Application.Usecase.ArFrame.DeleteFrame
{
    public record DeleteArFrameCommand(Guid FrameId) : IRequest;
}
