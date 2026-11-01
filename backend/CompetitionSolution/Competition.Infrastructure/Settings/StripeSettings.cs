namespace Competition.Infrastructure.Settings;

public class StripeSettings
{
    public const string SectionName = "Stripe";

    public string SecretKey { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public long MonthlyPriceCents { get; set; } = 499;
    public string Currency { get; set; } = "eur";
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}
