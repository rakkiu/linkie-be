using Domain.Interface;
using Application.Interfaces;
using Application.Model.Admin;
using Application.Model.WishwallAi;
using MediatR;

namespace Application.Usecase.ArFrame.DeleteFrame
{
    public class DeleteArFrameHandler : IRequestHandler<DeleteArFrameCommand>
    {
        private readonly IArFrameRepository _repo;

        public DeleteArFrameHandler(IArFrameRepository repo) => _repo = repo;

        public async Task Handle(DeleteArFrameCommand request, CancellationToken cancellationToken)
        {
            var frame = await _repo.GetByIdAsync(request.FrameId, cancellationToken)
                ?? throw new KeyNotFoundException($"AR Frame with id '{request.FrameId}' not found.");

            await _repo.DeleteAsync(frame, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);
        }
    }
}
