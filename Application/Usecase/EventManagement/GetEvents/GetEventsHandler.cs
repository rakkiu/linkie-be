using Domain.Enums;
using Domain.Interface;
using MediatR;

namespace Application.Usecase.EventManagement.GetEvents
{
    public class GetEventsHandler : IRequestHandler<GetEventsQuery, List<EventResponseDto>>
    {
        private readonly IEventRepository _repo;

        public GetEventsHandler(IEventRepository repo) => _repo = repo;

        public async Task<List<EventResponseDto>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
        {
            EventStatus status;
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse(request.Status, ignoreCase: true, out EventStatus parsed))
                status = parsed;
            else
                status = EventStatus.Ongoing;

            var events = await _repo.GetByStatusAsync(status, cancellationToken);

            return events.Select(e => new EventResponseDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                StartTime = new DateTimeOffset(e.StartTime, TimeSpan.Zero),
                EndTime = new DateTimeOffset(e.EndTime, TimeSpan.Zero),
                Location = e.Location,
                Status = e.Status.ToString(),
                ThumbnailUrl = e.ThumbnailUrl,
                CreatedAt = new DateTimeOffset(e.CreatedAt, TimeSpan.Zero)
            }).ToList();
        }
    }
}
