using Application.Usecase.ArFrame.GetFrames;
using Domain.Interface;
using MediatR;

namespace Application.Usecase.ArFrame.GetFrames
{
    public record GetAllArFramesQuery(Guid EventId) : IRequest<List<AdminArFrameDto>>;

    public class GetAllArFramesHandler : IRequestHandler<GetAllArFramesQuery, List<AdminArFrameDto>>
    {
        private readonly IArFrameRepository _repo;

        public GetAllArFramesHandler(IArFrameRepository repo) => _repo = repo;

        public async Task<List<AdminArFrameDto>> Handle(GetAllArFramesQuery request, CancellationToken cancellationToken)
        {
            var frames = await _repo.GetAllByEventIdAsync(request.EventId, cancellationToken);
            return frames.Select(f => new AdminArFrameDto
            {
                Id = f.Id,
                FrameName = f.FrameName,
                FrameUrl = f.FrameUrl,
                IsActive = f.IsActive,
                CreatedAt = f.CreatedAt
            }).ToList();
        }
    }
}
