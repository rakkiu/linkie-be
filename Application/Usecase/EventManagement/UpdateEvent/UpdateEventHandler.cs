using Application.Interfaces;
using Domain.Interface;
using MediatR;

namespace Application.Usecase.EventManagement.UpdateEvent
{
    public class UpdateEventHandler : IRequestHandler<UpdateEventCommand, UpdateEventResult>
    {
        private readonly IEventRepository _eventRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public UpdateEventHandler(IEventRepository eventRepository, ICloudinaryService cloudinaryService)
        {
            _eventRepository = eventRepository;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<UpdateEventResult> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
        {
            var @event = await _eventRepository.GetByIdAsync(request.Id, cancellationToken);
            if (@event == null)
            {
                throw new KeyNotFoundException($"Event with ID {request.Id} not found.");
            }

            if (request.Thumbnail != null && request.Thumbnail.Length > 0)
            {
                await using var stream = request.Thumbnail.OpenReadStream();
                @event.ThumbnailUrl = await _cloudinaryService.UploadImageAsync(stream, request.Thumbnail.FileName, cancellationToken);
            }

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
