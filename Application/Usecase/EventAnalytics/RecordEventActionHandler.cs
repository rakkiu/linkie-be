using Domain.Interface;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Usecase.EventAnalytics
{
    public class RecordEventActionHandler : IRequestHandler<RecordEventActionCommand, bool>
    {
        private readonly IEventRepository _repo;

        public RecordEventActionHandler(IEventRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> Handle(RecordEventActionCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = await _repo.GetByIdAsync(request.EventId, cancellationToken);
            if (eventEntity == null)
                return false;

            if (request.ActionType.Equals("share", StringComparison.OrdinalIgnoreCase))
            {
                eventEntity.TotalShares++;
            }
            else if (request.ActionType.Equals("timelapse", StringComparison.OrdinalIgnoreCase))
            {
                eventEntity.TotalTimelapses++;
            }
            else
            {
                return false;
            }

            await _repo.UpdateAsync(eventEntity, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
