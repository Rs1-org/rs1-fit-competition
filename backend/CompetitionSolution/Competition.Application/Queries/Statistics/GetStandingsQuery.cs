using Competition.Application.Common.Validation;
using Competition.Application.DTOs.StatisticDto;
using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.Statistics;

public class GetStandingsQuery : IRequest<StandingsDto>
{
    public string? CompetitionPublicId { get; init; }
    public string? SeasonPublicId { get; init; }
}

public class StandingsDto
{
    public IReadOnlyList<TeamResultsDto> Table { get; init; } = [];
    public IReadOnlyList<string> MatchPublicIds { get; init; } = [];
}

public class GetStandingsQueryValidator : AbstractValidator<GetStandingsQuery>
{
    public GetStandingsQueryValidator()
    {
        RuleFor(x => x.CompetitionPublicId).MustBeValidPublicId().When(x => x.CompetitionPublicId is not null);
        RuleFor(x => x.SeasonPublicId).MustBeValidPublicId().When(x => x.SeasonPublicId is not null);
    }
}

public class GetStandingsQueryHandler(IStatistic statistics) : IRequestHandler<GetStandingsQuery, StandingsDto>
{
    public async Task<StandingsDto> Handle(GetStandingsQuery request, CancellationToken cancellationToken)
    {
        var filter = new ChartFilter(request.CompetitionPublicId, request.SeasonPublicId);
        var matches = await statistics.GetScoredMatchesAsync(filter, cancellationToken);
        var ids = await statistics.GetMatchPublicIdsAsync(filter, cancellationToken);
        return new StandingsDto
        {
            Table = GetStatisticChartsQueryHandler.TeamResults(matches, null),
            MatchPublicIds = ids,
        };
    }
}
