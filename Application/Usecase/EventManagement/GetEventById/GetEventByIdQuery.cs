using MediatR;

namespace Application.Usecase.EventManagement.GetEventById
{
    public record GetEventByIdQuery(Guid Id) : IRequest<EventDetailDto?>;

    public class EventDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string? Location { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int MaxParticipants { get; set; }
        public bool IsWishwallEnabled { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
