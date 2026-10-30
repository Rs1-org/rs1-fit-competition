using Competition.Domain.Entities;
using Competition.Domain.Entities.Payments;
using NodaTime;
using NodaTime.Text;

namespace Competition.Application.Payments;

public record CheckoutSessionDto(string Url);
public record PremiumStatusDto(bool IsPremium, string? PremiumUntil, string? LatestPaymentStatus);
public record PremiumPricingDto(long MonthlyPriceCents, string Currency);

public record CheckoutSession(string SessionId, string Url, long AmountCents, string Currency);

public enum PaymentWebhookOutcome { Paid, Expired }
public record PaymentWebhookEvent(PaymentWebhookOutcome Outcome, string SessionId, string? PaymentIntentId);

/// <summary>The card processor, hidden from the Application layer.</summary>
public interface IPaymentGateway
{
    PremiumPricingDto GetPricing();
    Task<CheckoutSession> CreateCheckoutAsync(string? customerEmail, int months, CancellationToken cancellationToken);
    /// <summary>Verifies the signature. Returns null for event types that need no action.</summary>
    PaymentWebhookEvent? ParseWebhook(string payload, string signature);
}

public interface IPaymentStore
{
    Task<ApplicationUser?> FindUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<PaymentTransaction?> FindBySessionAsync(string sessionId, CancellationToken cancellationToken);
    Task<PaymentTransaction?> GetLatestAsync(Guid userId, CancellationToken cancellationToken);
    Task AddAsync(PaymentTransaction payment, CancellationToken cancellationToken);
}

public interface IPremiumAccess
{
    Task<bool> HasAccessAsync(Guid userId, CancellationToken cancellationToken);
}

public static class PremiumPlan
{
    public const int DaysPerMonth = 30;
    public static readonly int[] AllowedMonths = [1, 3, 12];

    /// <summary>A renewal adds to the time left; an expired subscription restarts from now.</summary>
    public static Instant Extend(Instant? current, Instant now, int months)
    {
        var start = current.HasValue && current.Value > now ? current.Value : now;
        return start + Duration.FromDays(months * DaysPerMonth);
    }

    public static string? Format(Instant? until) => until.HasValue ? InstantPattern.ExtendedIso.Format(until.Value) : null;
}
