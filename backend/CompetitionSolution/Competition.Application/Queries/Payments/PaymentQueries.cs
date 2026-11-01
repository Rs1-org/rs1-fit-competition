using Competition.Application.Auth.Interfaces;
using Competition.Application.Payments;
using MediatR;
using NodaTime;

namespace Competition.Application.Queries.Payments;

public record GetPremiumPricingQuery : IRequest<PremiumPricingDto>;

public class GetPremiumPricingQueryHandler(IPaymentGateway gateway) : IRequestHandler<GetPremiumPricingQuery, PremiumPricingDto>
{
    public Task<PremiumPricingDto> Handle(GetPremiumPricingQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(gateway.GetPricing());
}

public record GetPremiumStatusQuery : IRequest<PremiumStatusDto>;

public class GetPremiumStatusQueryHandler(ICurrentUser currentUser, IPaymentStore store, IClock clock)
    : IRequestHandler<GetPremiumStatusQuery, PremiumStatusDto>
{
    public async Task<PremiumStatusDto> Handle(GetPremiumStatusQuery request, CancellationToken cancellationToken)
    {
        var user = await store.FindUserAsync(currentUser.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Sign in is required.");
        var latest = await store.GetLatestAsync(user.Id, cancellationToken);
        return new PremiumStatusDto(user.PremiumUntil > clock.GetCurrentInstant(), PremiumPlan.Format(user.PremiumUntil),
            latest?.Status.ToString());
    }
}
