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
        public bool IsEmailVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Handle đăng nhập duy nhất, chỉ dùng cho tài khoản Organizer do Admin tạo.</summary>
        public string? Username { get; set; }

        /// <summary>Event ID mà Organizer được phép xem Dashboard. Null nếu không phải Organizer.</summary>
        public Guid? ManagedEventId { get; set; }
        
        /// <summary>Gói dịch vụ cấp cho Organizer (Students, Small, Medium, Large).</summary>
        public PlanTier PlanTier { get; set; } = PlanTier.Medium;

        public virtual Event? ManagedEvent { get; set; }

        public ICollection<JwtToken> JwtTokens { get; set; } = new List<JwtToken>();
        public ICollection<WishwallMessage> WishwallMessages { get; set; } = new List<WishwallMessage>();
        public ICollection<FrameUsage> FrameUsages { get; set; } = new List<FrameUsage>();
        public ICollection<EventParticipant> EventParticipants { get; set; } = new List<EventParticipant>();
        public ICollection<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();
        public ICollection<UserEventStat> UserEventStats { get; set; } = new List<UserEventStat>();
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}

