using Domain.Interface;
using MediatR;

namespace Application.Usecase.EventManagement.GetAdminEventList
{
    public class GetAdminEventListHandler : IRequestHandler<GetAdminEventListQuery, List<AdminEventDto>>
    {
        private readonly IEventRepository _eventRepository;

        public GetAdminEventListHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<List<AdminEventDto>> Handle(GetAdminEventListQuery request, CancellationToken cancellationToken)
        {
            var events = await _eventRepository.GetAllAsync(cancellationToken);
            return events.Select(e => new AdminEventDto(
                e.Id,
                e.Name,
                e.ThumbnailUrl,
                new DateTimeOffset(DateTime.SpecifyKind(e.StartTime, DateTimeKind.Utc)),
                new DateTimeOffset(DateTime.SpecifyKind(e.EndTime, DateTimeKind.Utc)),
                e.Location,
                e.MaxParticipants,
                e.IsWishwallEnabled,
                e.Status,
                new DateTimeOffset(DateTime.SpecifyKind(e.CreatedAt, DateTimeKind.Utc))
            )).ToList();
        }
    }
}
