namespace Domain.Entity
{
    public class UserEventStat
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public int TotalPhotos { get; set; }
        public int TotalMessages { get; set; }
        public float EngagementScore { get; set; }

        public Event Event { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
