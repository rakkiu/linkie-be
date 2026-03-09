namespace Domain.Entity
{
    public class FrameUsage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public Guid FrameId { get; set; }
        public DateTime UsedAt { get; set; } = DateTime.UtcNow;

        public Event Event { get; set; } = null!;
        public User User { get; set; } = null!;
        public ArFrame Frame { get; set; } = null!;
    }
}
