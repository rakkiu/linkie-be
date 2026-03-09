using Domain.Enums;

namespace Domain.Entity
{
    public class Event
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Location { get; set; }
        public EventStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<WishwallMessage> WishwallMessages { get; set; } = new List<WishwallMessage>();
        public ICollection<ArFrame> ArFrames { get; set; } = new List<ArFrame>();
        public ICollection<FrameUsage> FrameUsages { get; set; } = new List<FrameUsage>();
        public ICollection<WishwallKeyword> WishwallKeywords { get; set; } = new List<WishwallKeyword>();
        public ICollection<EventParticipant> EventParticipants { get; set; } = new List<EventParticipant>();
        public ICollection<UserEventStat> UserEventStats { get; set; } = new List<UserEventStat>();
    }
}
