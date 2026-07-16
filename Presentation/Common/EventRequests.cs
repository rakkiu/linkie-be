using Domain.Enums;

namespace Presentation.Common
{
    public class CreateEventRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string? Location { get; set; }
        public int MaxParticipants { get; set; }
        public bool IsWishwallEnabled { get; set; }
        public EventStatus Status { get; set; }
        public string? ThumbnailUrl { get; set; }
    }

    public class UpdateEventRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string? Location { get; set; }
        public int MaxParticipants { get; set; }
        public bool IsWishwallEnabled { get; set; }
        public EventStatus Status { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}
