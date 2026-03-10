using Domain.Interface;
using MediatR;

namespace Application.Usecase.EventManagement.GetEventById
{
    public class GetEventByIdHandler : IRequestHandler<GetEventByIdQuery, EventDetailDto?>
    {
        private readonly IEventRepository _repo;

        public GetEventByIdHandler(IEventRepository repo) => _repo = repo;

        public async Task<EventDetailDto?> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
        {
            var @event = await _repo.GetByIdAsync(request.Id, cancellationToken);
            if (@event == null)
                return null;

            return new EventDetailDto
            {
                Id = @event.Id,
                Name = @event.Name,
                Description = @event.Description,
                StartTime = new DateTimeOffset(@event.StartTime, TimeSpan.Zero),
                EndTime = new DateTimeOffset(@event.EndTime, TimeSpan.Zero),
                Location = @event.Location,
                ThumbnailUrl = @event.ThumbnailUrl,
                MaxParticipants = @event.MaxParticipants,
                IsWishwallEnabled = @event.IsWishwallEnabled,
                Status = @event.Status.ToString(),
                CreatedAt = new DateTimeOffset(@event.CreatedAt, TimeSpan.Zero)
            };
        }
    }
}
