using Competition.Application.Common.Validation;
using Competition.Application.DTOs.SelectionDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.Person;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.SelectionQueries.GetPlayerAppearances;

public class GetPlayerAppearancesQuery : IRequest<IReadOnlyList<PlayerAppearanceDto>>
{
    public string PublicId { get; set; } = string.Empty;
}

public class GetPlayerAppearancesQueryValidator : AbstractValidator<GetPlayerAppearancesQuery>
{
    public GetPlayerAppearancesQueryValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}

public class GetPlayerAppearancesQueryHandler(IPlayerAppearances appearances, IPublicIdLookup lookup)
    : IRequestHandler<GetPlayerAppearancesQuery, IReadOnlyList<PlayerAppearanceDto>>
{
    public async Task<IReadOnlyList<PlayerAppearanceDto>> Handle(GetPlayerAppearancesQuery request, CancellationToken cancellationToken)
    {
        var playerId = await lookup.GetRequiredIdAsync<Player>(request.PublicId, cancellationToken);
        return await appearances.ForPlayerAsync(playerId, cancellationToken);
    }
}
