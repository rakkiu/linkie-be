using Domain.Enums;

namespace Domain.Entity
{
    public class WishwallMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public WishwallSentiment Sentiment { get; set; }
        public bool IsApproved { get; set; } = false;
        public bool IsHidden { get; set; }
        public bool IsDisplayedOnLed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Event Event { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
