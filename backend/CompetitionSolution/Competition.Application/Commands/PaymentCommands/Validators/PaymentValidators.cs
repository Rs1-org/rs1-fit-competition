using Competition.Application.Payments;
using FluentValidation;

namespace Competition.Application.Commands.PaymentCommands.Validators;

public class CreateCheckoutSessionCommandValidator : AbstractValidator<CreateCheckoutSessionCommand>
{
    public CreateCheckoutSessionCommandValidator() =>
        RuleFor(x => x.Months).Must(x => PremiumPlan.AllowedMonths.Contains(x))
            .WithMessage("Choose a 1, 3 or 12 month plan.");
}

public class HandleStripeWebhookCommandValidator : AbstractValidator<HandleStripeWebhookCommand>
{
    public HandleStripeWebhookCommandValidator()
    {
        RuleFor(x => x.Payload).NotEmpty();
        RuleFor(x => x.Signature).NotEmpty();
    }
}
