using Domain.Enums;

namespace Domain.Entity
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? FirebaseUid { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<JwtToken> JwtTokens { get; set; } = new List<JwtToken>();
        public ICollection<WishwallMessage> WishwallMessages { get; set; } = new List<WishwallMessage>();
        public ICollection<FrameUsage> FrameUsages { get; set; } = new List<FrameUsage>();
        public ICollection<EventParticipant> EventParticipants { get; set; } = new List<EventParticipant>();
        public ICollection<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();
        public ICollection<UserEventStat> UserEventStats { get; set; } = new List<UserEventStat>();
    }
}
