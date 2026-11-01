using Competition.Application.Auth.Interfaces;
using Competition.Application.Interfaces;
using Competition.Application.Payments;
using Competition.Domain.Entities.Payments;
using MediatR;
using NodaTime;

namespace Competition.Application.Commands.PaymentCommands;

public record CreateCheckoutSessionCommand(int Months) : IRequest<CheckoutSessionDto>;

public class CreateCheckoutSessionCommandHandler(ICurrentUser currentUser, IPaymentStore store, IPaymentGateway gateway, IUnitOfWork unit)
    : IRequestHandler<CreateCheckoutSessionCommand, CheckoutSessionDto>
{
    public async Task<CheckoutSessionDto> Handle(CreateCheckoutSessionCommand request, CancellationToken cancellationToken)
    {
        var user = await store.FindUserAsync(currentUser.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Sign in is required.");
        var session = await gateway.CreateCheckoutAsync(user.Email, request.Months, cancellationToken);
        await store.AddAsync(new PaymentTransaction
        {
            UserId = user.Id, AmountCents = session.AmountCents, Currency = session.Currency,
            Months = request.Months, StripeSessionId = session.SessionId,
        }, cancellationToken);
        await unit.SaveChangesAsync(cancellationToken);
        return new CheckoutSessionDto(session.Url);
    }
}

public record HandleStripeWebhookCommand(string Payload, string Signature) : IRequest<Unit>;

public class HandleStripeWebhookCommandHandler(IPaymentStore store, IPaymentGateway gateway, IUnitOfWork unit, IClock clock)
    : IRequestHandler<HandleStripeWebhookCommand, Unit>
{
    public async Task<Unit> Handle(HandleStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        var stripeEvent = gateway.ParseWebhook(request.Payload, request.Signature);
        if (stripeEvent is null) return Unit.Value;

        var payment = await store.FindBySessionAsync(stripeEvent.SessionId, cancellationToken);
        // Stripe redelivers events, so anything already settled is left alone.
        if (payment is null || payment.Status != PaymentStatus.Pending) return Unit.Value;

        if (stripeEvent.Outcome == PaymentWebhookOutcome.Paid)
        {
            var now = clock.GetCurrentInstant();
            payment.Status = PaymentStatus.Paid;
            payment.PaidAt = now;
            payment.StripePaymentIntentId = stripeEvent.PaymentIntentId;
            payment.User.PremiumUntil = PremiumPlan.Extend(payment.User.PremiumUntil, now, payment.Months);
        }
        else
            payment.Status = PaymentStatus.Expired;

        // A lost concurrency race surfaces as a 500, so Stripe retries and hits the settled check above.
        await unit.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
