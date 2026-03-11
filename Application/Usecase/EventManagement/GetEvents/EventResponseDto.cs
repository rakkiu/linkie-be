namespace Application.Usecase.EventManagement.GetEvents
{
    public class EventResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string? Location { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
