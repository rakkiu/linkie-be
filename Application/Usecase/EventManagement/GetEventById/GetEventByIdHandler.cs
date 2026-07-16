using Domain.Interface;
using Application.Interfaces;
using Application.Model.Admin;
using Application.Model.WishwallAi;
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
                StartTime = new DateTimeOffset(DateTime.SpecifyKind(@event.StartTime, DateTimeKind.Utc)),
                EndTime = new DateTimeOffset(DateTime.SpecifyKind(@event.EndTime, DateTimeKind.Utc)),
                Location = @event.Location,
                ThumbnailUrl = @event.ThumbnailUrl,
                MaxParticipants = @event.MaxParticipants,
                IsWishwallEnabled = @event.IsWishwallEnabled,
                Status = @event.Status.ToString(),
                CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(@event.CreatedAt, DateTimeKind.Utc))
            };
        }
    }
}
