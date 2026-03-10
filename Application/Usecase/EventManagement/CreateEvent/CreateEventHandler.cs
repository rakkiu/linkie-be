using Application.Interfaces;
using Domain.Entity;
using Domain.Enums;
using Domain.Interface;
using MediatR;

namespace Application.Usecase.EventManagement.CreateEvent
{
    public class CreateEventHandler : IRequestHandler<CreateEventCommand, CreateEventResult>
    {
        private readonly IEventRepository _eventRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public CreateEventHandler(IEventRepository eventRepository, ICloudinaryService cloudinaryService)
        {
            _eventRepository = eventRepository;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<CreateEventResult> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            string? thumbnailUrl = null;

            if (request.Thumbnail != null && request.Thumbnail.Length > 0)
            {
                await using var stream = request.Thumbnail.OpenReadStream();
                thumbnailUrl = await _cloudinaryService.UploadImageAsync(stream, request.Thumbnail.FileName, cancellationToken);
            }

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
                ThumbnailUrl = thumbnailUrl,
                Status = EventStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            await _eventRepository.AddAsync(@event, cancellationToken);
            await _eventRepository.SaveChangesAsync(cancellationToken);

            return new CreateEventResult(@event.Id, @event.Name, @event.ThumbnailUrl);
        }
    }
}
