using Competition.Application.Auth.Interfaces;
using Competition.Application.Exeptions;
using Competition.Application.Payments;
using MediatR;

namespace Competition.Application.Behaviors;

/// <summary>Marks a request that only an active premium subscriber may run.</summary>
public interface IRequiresPremium;

/// <summary>Checks the database, not the JWT claim, which can be up to an hour out of date.</summary>
public class PremiumBehavior<TRequest, TResponse>(ICurrentUser currentUser, IPremiumAccess premium) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IRequiresPremium && !await premium.HasAccessAsync(currentUser.UserId, cancellationToken))
            throw new ForbiddenAccessException("An active premium subscription is required.");
        return await next(cancellationToken);
    }
}
