using Competition.Application.Behaviors;
using Competition.Application.Common.Models;
using Competition.Application.Common.Validation;
using Competition.Application.Interfaces;
using Competition.Application.Payments;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.Statistics.Advanced;

public record TeamFormMatchDto(string Date, string OpponentName, int GoalsFor, int GoalsAgainst, string Result);
public record TeamFormDto(string SelectionPublicId, string SelectionName, int Wins, int Draws, int Losses,
    int GoalsFor, int GoalsAgainst, int Points, IReadOnlyList<TeamFormMatchDto> Matches);
public record HeadToHeadMatchDto(string Date, string HomeName, int HomeScore, string AwayName, int AwayScore);
public record HeadToHeadDto(string FirstName, string SecondName, int FirstWins, int Draws, int SecondWins,
    int FirstGoals, int SecondGoals, IReadOnlyList<HeadToHeadMatchDto> Matches);

public class GetTeamFormQuery : IRequest<TeamFormDto>, IRequiresPremium
{
    public string SelectionPublicId { get; init; } = string.Empty;
    public int Last { get; init; } = 5;
    public string? CompetitionPublicId { get; init; }
    public string? SeasonPublicId { get; init; }
}

public class GetTeamFormQueryValidator : AbstractValidator<GetTeamFormQuery>
{
    public GetTeamFormQueryValidator()
    {
        RuleFor(x => x.SelectionPublicId).MustBeValidPublicId();
        RuleFor(x => x.Last).InclusiveBetween(1, 20);
        RuleFor(x => x.CompetitionPublicId!).MustBeValidPublicId().When(x => x.CompetitionPublicId is not null);
        RuleFor(x => x.SeasonPublicId!).MustBeValidPublicId().When(x => x.SeasonPublicId is not null);
    }
}

public class GetTeamFormQueryHandler(IStatistic statistics) : IRequestHandler<GetTeamFormQuery, TeamFormDto>
{
    public async Task<TeamFormDto> Handle(GetTeamFormQuery request, CancellationToken cancellationToken)
    {
        var filter = new ChartFilter(request.CompetitionPublicId, request.SeasonPublicId);
        var rows = await statistics.GetTeamMatchesAsync(request.SelectionPublicId, request.Last, filter, cancellationToken);
        return BuildForm(request.SelectionPublicId, rows);
    }

    public static TeamFormDto BuildForm(string selectionPublicId, IReadOnlyList<AdvancedMatchRow> rows)
    {
        var matches = rows.Select(x =>
        {
            var isHome = x.HomePublicId == selectionPublicId;
            int goalsFor = isHome ? x.HomeScore : x.AwayScore, goalsAgainst = isHome ? x.AwayScore : x.HomeScore;
            return new TeamFormMatchDto(PremiumPlan.Format(x.Date)!, isHome ? x.AwayName : x.HomeName, goalsFor, goalsAgainst,
                goalsFor > goalsAgainst ? "W" : goalsFor == goalsAgainst ? "D" : "L");
        }).ToList();
        int wins = matches.Count(x => x.Result == "W"), draws = matches.Count(x => x.Result == "D");
        var name = rows.Count == 0 ? string.Empty
            : rows[0].HomePublicId == selectionPublicId ? rows[0].HomeName : rows[0].AwayName;
        return new TeamFormDto(selectionPublicId, name, wins, draws, matches.Count - wins - draws,
            matches.Sum(x => x.GoalsFor), matches.Sum(x => x.GoalsAgainst), wins * 3 + draws, matches);
    }
}

public class GetHeadToHeadQuery : IRequest<HeadToHeadDto>, IRequiresPremium
{
    public const int MaxMatches = 10;
    public string FirstSelectionPublicId { get; init; } = string.Empty;
    public string SecondSelectionPublicId { get; init; } = string.Empty;
    public string? CompetitionPublicId { get; init; }
    public string? SeasonPublicId { get; init; }
}

public class GetHeadToHeadQueryValidator : AbstractValidator<GetHeadToHeadQuery>
{
    public GetHeadToHeadQueryValidator()
    {
        RuleFor(x => x.FirstSelectionPublicId).MustBeValidPublicId();
        RuleFor(x => x.SecondSelectionPublicId).MustBeValidPublicId()
            .NotEqual(x => x.FirstSelectionPublicId).WithMessage("Choose two different teams.");
        RuleFor(x => x.CompetitionPublicId!).MustBeValidPublicId().When(x => x.CompetitionPublicId is not null);
        RuleFor(x => x.SeasonPublicId!).MustBeValidPublicId().When(x => x.SeasonPublicId is not null);
    }
}

public class GetHeadToHeadQueryHandler(IStatistic statistics) : IRequestHandler<GetHeadToHeadQuery, HeadToHeadDto>
{
    public async Task<HeadToHeadDto> Handle(GetHeadToHeadQuery request, CancellationToken cancellationToken)
    {
        var filter = new ChartFilter(request.CompetitionPublicId, request.SeasonPublicId);
        var rows = await statistics.GetHeadToHeadMatchesAsync(request.FirstSelectionPublicId, request.SecondSelectionPublicId, filter, cancellationToken);
        return BuildHeadToHead(request.FirstSelectionPublicId, rows);
    }

    public static HeadToHeadDto BuildHeadToHead(string firstPublicId, IReadOnlyList<AdvancedMatchRow> rows)
    {
        var scores = rows.Select(x => x.HomePublicId == firstPublicId ? (First: x.HomeScore, Second: x.AwayScore) : (First: x.AwayScore, Second: x.HomeScore)).ToList();
        var firstName = rows.Count == 0 ? string.Empty : rows[0].HomePublicId == firstPublicId ? rows[0].HomeName : rows[0].AwayName;
        var secondName = rows.Count == 0 ? string.Empty : rows[0].HomePublicId == firstPublicId ? rows[0].AwayName : rows[0].HomeName;
        return new HeadToHeadDto(firstName, secondName, scores.Count(x => x.First > x.Second), scores.Count(x => x.First == x.Second),
            scores.Count(x => x.First < x.Second), scores.Sum(x => x.First), scores.Sum(x => x.Second),
            rows.Take(GetHeadToHeadQuery.MaxMatches)
                .Select(x => new HeadToHeadMatchDto(PremiumPlan.Format(x.Date)!, x.HomeName, x.HomeScore, x.AwayName, x.AwayScore)).ToList());
    }
}

public class GetPlayerStatsQuery : IRequest<PagedResult<PlayerStatsRow>>, IRequiresPremium
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SelectionPublicId { get; init; }
    public string? CompetitionPublicId { get; init; }
    public string? SeasonPublicId { get; init; }
}

public class GetPlayerStatsQueryValidator : AbstractValidator<GetPlayerStatsQuery>
{
    public GetPlayerStatsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x.SelectionPublicId!).MustBeValidPublicId().When(x => x.SelectionPublicId is not null);
        RuleFor(x => x.CompetitionPublicId!).MustBeValidPublicId().When(x => x.CompetitionPublicId is not null);
        RuleFor(x => x.SeasonPublicId!).MustBeValidPublicId().When(x => x.SeasonPublicId is not null);
    }
}

public class GetPlayerStatsQueryHandler(IStatistic statistics) : IRequestHandler<GetPlayerStatsQuery, PagedResult<PlayerStatsRow>>
{
    public Task<PagedResult<PlayerStatsRow>> Handle(GetPlayerStatsQuery request, CancellationToken cancellationToken) =>
        statistics.GetPlayerStatsAsync(request.SelectionPublicId, new ChartFilter(request.CompetitionPublicId, request.SeasonPublicId),
            request.Page, request.PageSize, cancellationToken);
}
