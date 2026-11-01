using Competition.Application.Helpers;
using Competition.Application.Payments;
using Competition.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Competition.Infrastructure.Services;

public class StripePaymentGateway(IOptions<StripeSettings> stripeOptions) : IPaymentGateway
{
    private const string PaidStatus = "paid";
    private readonly StripeSettings settings = stripeOptions.Value;

    public PremiumPricingDto GetPricing() => new(settings.MonthlyPriceCents, settings.Currency);

    public async Task<CheckoutSession> CreateCheckoutAsync(string? customerEmail, int months, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.SecretKey))
            throw BusinessRule.Violation("Payment", "Payments are not configured.");

        var amount = settings.MonthlyPriceCents * months;
        var service = new SessionService(new StripeClient(settings.SecretKey));
        var options = new SessionCreateOptions
        {
            Mode = "payment",
            CustomerEmail = customerEmail,
            SuccessUrl = settings.SuccessUrl,
            CancelUrl = settings.CancelUrl,
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = settings.Currency,
                        UnitAmount = amount,
                        ProductData = new SessionLineItemPriceDataProductDataOptions { Name = $"Premium statistics ({months} months)" },
                    },
                },
            ],
        };
        try
        {
            var session = await service.CreateAsync(options, cancellationToken: cancellationToken);
            return new CheckoutSession(session.Id, session.Url, amount, settings.Currency);
        }
        catch (StripeException)
        {
            throw BusinessRule.Violation("Payment", "The payment could not be started. Please try again.");
        }
    }

    public PaymentWebhookEvent? ParseWebhook(string payload, string signature)
    {
        // An empty secret would let anyone compute a valid signature.
        if (string.IsNullOrWhiteSpace(settings.WebhookSecret))
            throw BusinessRule.Violation("Signature", "Payments are not configured.");

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(payload, signature, settings.WebhookSecret, throwOnApiVersionMismatch: false);
        }
        catch (StripeException)
        {
            throw BusinessRule.Violation("Signature", "The Stripe signature is not valid.");
        }

        if (stripeEvent.Data.Object is not Session session) return null;
        return stripeEvent.Type switch
        {
            EventTypes.CheckoutSessionCompleted when session.PaymentStatus == PaidStatus =>
                new PaymentWebhookEvent(PaymentWebhookOutcome.Paid, session.Id, session.PaymentIntentId),
            EventTypes.CheckoutSessionExpired =>
                new PaymentWebhookEvent(PaymentWebhookOutcome.Expired, session.Id, null),
            _ => null,
        };
    }
}
