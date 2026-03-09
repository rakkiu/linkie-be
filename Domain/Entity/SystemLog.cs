namespace Domain.Entity
{
    public class SystemLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Action { get; set; } = string.Empty;
        public Guid AdminId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User Admin { get; set; } = null!;
    }
}
