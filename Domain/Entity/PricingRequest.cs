using Domain.Enums;
using System;

namespace Domain.Entity
{
    public class PricingRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string? Fanpage { get; set; }
        public string PlanId { get; set; } = string.Empty;
        public PricingRequestStatus Status { get; set; } = PricingRequestStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
