using Domain.Entity;
using Domain.Interface;
using Application.Interfaces;
using Application.Model.Admin;
using Application.Model.WishwallAi;
using MediatR;

namespace Application.Usecase.ArFrame.RecordUsage
{
    public class RecordFrameUsageHandler : IRequestHandler<RecordFrameUsageCommand, bool>
    {
        private readonly IArFrameRepository _repo;

        public RecordFrameUsageHandler(IArFrameRepository repo) => _repo = repo;

        public async Task<bool> Handle(RecordFrameUsageCommand request, CancellationToken cancellationToken)
        {
            var frame = await _repo.GetByIdAsync(request.FrameId, cancellationToken);
            if (frame == null || frame.EventId != request.EventId)
                throw new KeyNotFoundException("AR frame not found for this event.");

            var usage = new FrameUsage
            {
                EventId = request.EventId,
                FrameId = request.FrameId,
                UserId = request.UserId,
                UsedAt = DateTime.UtcNow
            };

            await _repo.AddUsageAsync(usage, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
