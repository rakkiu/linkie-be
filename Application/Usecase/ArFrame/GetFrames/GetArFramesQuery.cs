using MediatR;

namespace Application.Usecase.ArFrame.GetFrames
{
    public record GetArFramesQuery(Guid EventId) : IRequest<List<ArFrameDto>>;
}
