using Domain.Interface;
using MediatR;

namespace Application.Usecase.ArFrame.GetFrames
{
    public class GetArFramesHandler : IRequestHandler<GetArFramesQuery, List<ArFrameDto>>
    {
        private readonly IArFrameRepository _repo;

        public GetArFramesHandler(IArFrameRepository repo) => _repo = repo;

        public async Task<List<ArFrameDto>> Handle(GetArFramesQuery request, CancellationToken cancellationToken)
        {
            var frames = await _repo.GetActiveByEventIdAsync(request.EventId, cancellationToken);

            return frames.Select(f => new ArFrameDto
            {
                Id = f.Id,
                FrameName = f.FrameName,
                FrameUrl = f.FrameUrl
            }).ToList();
        }
    }
}
