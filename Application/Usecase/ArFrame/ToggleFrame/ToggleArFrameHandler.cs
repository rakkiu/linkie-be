using Domain.Interface;
using MediatR;

namespace Application.Usecase.ArFrame.ToggleFrame
{
    public class ToggleArFrameHandler : IRequestHandler<ToggleArFrameCommand, ToggleArFrameResult>
    {
        private readonly IArFrameRepository _repo;

        public ToggleArFrameHandler(IArFrameRepository repo) => _repo = repo;

        public async Task<ToggleArFrameResult> Handle(ToggleArFrameCommand request, CancellationToken cancellationToken)
        {
            var frame = await _repo.GetByIdAsync(request.FrameId, cancellationToken)
                ?? throw new KeyNotFoundException($"AR Frame with id '{request.FrameId}' not found.");

            frame.IsActive = !frame.IsActive;
            await _repo.SaveChangesAsync(cancellationToken);

            return new ToggleArFrameResult(frame.Id, frame.IsActive);
        }
    }
}
