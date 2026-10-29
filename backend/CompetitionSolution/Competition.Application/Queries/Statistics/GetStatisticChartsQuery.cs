using Competition.Application.Common.Validation;
using Competition.Application.DTOs.StatisticDto;
using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.Statistics;

public class GetStatisticChartsQuery : IRequest<StatisticChartsDto>
{
    public string? CompetitionPublicId { get; init; }
    public string? SeasonPublicId { get; init; }
}

public class GetStatisticChartsQueryValidator : AbstractValidator<GetStatisticChartsQuery>
{
    public GetStatisticChartsQueryValidator()
    {
        RuleFor(x => x.CompetitionPublicId).MustBeValidPublicId().When(x => x.CompetitionPublicId is not null);
        RuleFor(x => x.SeasonPublicId).MustBeValidPublicId().When(x => x.SeasonPublicId is not null);
    }
}

public class GetStatisticChartsQueryHandler(IStatistic statistics) : IRequestHandler<GetStatisticChartsQuery, StatisticChartsDto>
{
    public const int MaxTeams = 10;

    public async Task<StatisticChartsDto> Handle(GetStatisticChartsQuery request, CancellationToken cancellationToken)
    {
        var filter = new ChartFilter(request.CompetitionPublicId, request.SeasonPublicId);
        var matches = await statistics.GetChartMatchesAsync(filter, cancellationToken);
        var shots = await statistics.GetChartTeamShotsAsync(filter, cancellationToken);
        return new StatisticChartsDto
        {
            GoalsPerRound = GoalsPerRound(matches),
            TeamResults = TeamResults(matches),
            TeamShots = TeamShots(shots),
        };
    }

    public static IReadOnlyList<RoundGoalsDto> GoalsPerRound(IEnumerable<ChartMatchRow> matches) => matches
        .Where(x => x.RoundNumber.HasValue)
        .GroupBy(x => x.RoundNumber!.Value)
        .OrderBy(g => g.Key)
        .Select(g => new RoundGoalsDto(g.Key, g.First().RoundName ?? $"Round {g.Key}", g.Count(),
            g.Sum(x => x.HomeScore + x.AwayScore)))
        .ToList();

    public static IReadOnlyList<TeamResultsDto> TeamResults(IEnumerable<ChartMatchRow> matches) => matches
        .SelectMany(x => new[]
        {
            (Id: x.HomePublicId, Name: x.HomeName, For: x.HomeScore, Against: x.AwayScore),
            (Id: x.AwayPublicId, Name: x.AwayName, For: x.AwayScore, Against: x.HomeScore),
        })
        .GroupBy(x => x.Id)
        .Select(g =>
        {
            int wins = g.Count(x => x.For > x.Against), draws = g.Count(x => x.For == x.Against);
            return new TeamResultsDto(g.Key, g.First().Name, g.Count(), wins, draws, g.Count() - wins - draws,
                g.Sum(x => x.For), g.Sum(x => x.Against), wins * 3 + draws);
        })
        .OrderByDescending(x => x.Points).ThenByDescending(x => x.GoalsFor - x.GoalsAgainst)
        .ThenByDescending(x => x.GoalsFor).ThenBy(x => x.SelectionName)
        .Take(MaxTeams)
        .ToList();

    public static IReadOnlyList<TeamShotsDto> TeamShots(IEnumerable<ChartTeamShotRow> rows) => rows
        .GroupBy(x => x.SelectionPublicId)
        .Select(g => new TeamShotsDto(g.Key, g.First().SelectionName, g.Count(),
            Math.Round(g.Average(x => x.TotalShots), 1), Math.Round(g.Average(x => x.ShotsOnTarget), 1),
            Math.Round(g.Average(x => x.Possession), 1)))
        .OrderByDescending(x => x.AverageShots).ThenBy(x => x.SelectionName)
        .Take(MaxTeams)
        .ToList();
}
