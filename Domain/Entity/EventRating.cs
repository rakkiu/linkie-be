using System;

namespace Domain.Entity
{
    public class EventRating
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public int StarRating { get; set; }
        public DateTime CreatedAt { get; set; }

        public Event Event { get; set; }
        public User User { get; set; }
    }
}
