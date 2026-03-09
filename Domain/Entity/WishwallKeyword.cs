namespace Domain.Entity
{
    public class WishwallKeyword
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EventId { get; set; }
        public string Keyword { get; set; } = string.Empty;
        public int Frequency { get; set; }

        public Event Event { get; set; } = null!;
    }
}
