using Domain.Enums;

namespace Domain.Entity
{
    public class Ticket
    {
        public Guid TicketId { get; set; } = Guid.NewGuid();
        public Guid EventId { get; set; }
        public string TicketCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public Guid? UserId { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Event Event { get; set; } = null!;
        public User? User { get; set; }
    }
}
