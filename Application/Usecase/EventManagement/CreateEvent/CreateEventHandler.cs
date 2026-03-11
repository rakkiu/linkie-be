using Domain.Entity;
using Domain.Enums;
using Domain.Interface;
using MediatR;

namespace Application.Usecase.EventManagement.CreateEvent
{
    public class CreateEventHandler : IRequestHandler<CreateEventCommand, CreateEventResult>
    {
        private readonly IEventRepository _eventRepository;

        public CreateEventHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<CreateEventResult> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            var @event = new Event
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                StartTime = request.StartTime.UtcDateTime,
                EndTime = request.EndTime.UtcDateTime,
                Location = request.Location,
                MaxParticipants = request.MaxParticipants,
                IsWishwallEnabled = request.IsWishwallEnabled,
                ThumbnailUrl = request.ThumbnailUrl,
                Status = EventStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            await _eventRepository.AddAsync(@event, cancellationToken);
            await _eventRepository.SaveChangesAsync(cancellationToken);

            return new CreateEventResult(@event.Id, @event.Name, @event.ThumbnailUrl);
        }
    }
}
