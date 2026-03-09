namespace Domain.Entity
{
    public class ArFrame
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EventId { get; set; }
        public string FrameName { get; set; } = string.Empty;
        public string FrameUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Event Event { get; set; } = null!;
        public ICollection<FrameUsage> FrameUsages { get; set; } = new List<FrameUsage>();
    }
}
