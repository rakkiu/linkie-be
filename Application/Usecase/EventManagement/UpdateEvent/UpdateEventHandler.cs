using Domain.Interface;
using MediatR;

namespace Application.Usecase.EventManagement.UpdateEvent
{
    public class UpdateEventHandler : IRequestHandler<UpdateEventCommand, UpdateEventResult>
    {
        private readonly IEventRepository _eventRepository;

        public UpdateEventHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<UpdateEventResult> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
        {
            var @event = await _eventRepository.GetByIdAsync(request.Id, cancellationToken);
            if (@event == null)
            {
                throw new KeyNotFoundException($"Event with ID {request.Id} not found.");
            }

            if (request.ThumbnailUrl != null)
                @event.ThumbnailUrl = request.ThumbnailUrl;

            @event.Name = request.Name;
            @event.Description = request.Description;
            @event.StartTime = request.StartTime.UtcDateTime;
            @event.EndTime = request.EndTime.UtcDateTime;
            @event.Location = request.Location;
            @event.MaxParticipants = request.MaxParticipants;
            @event.IsWishwallEnabled = request.IsWishwallEnabled;
            @event.Status = request.Status;

            await _eventRepository.UpdateAsync(@event, cancellationToken);
            await _eventRepository.SaveChangesAsync(cancellationToken);

            return new UpdateEventResult(@event.Id, @event.Name, @event.ThumbnailUrl);
        }
    }
}
