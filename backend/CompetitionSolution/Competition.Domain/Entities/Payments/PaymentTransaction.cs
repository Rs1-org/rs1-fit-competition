using NodaTime;

namespace Competition.Domain.Entities.Payments;

public enum PaymentStatus
{
    Pending,
    Paid,
    Expired,
}

/// <summary>One Stripe Checkout attempt; a Paid row is what activated or extended the user's premium.</summary>
public class PaymentTransaction : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public long AmountCents { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int Months { get; set; }
    public string StripeSessionId { get; set; } = string.Empty;
    public string? StripePaymentIntentId { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public Instant? PaidAt { get; set; }
}
