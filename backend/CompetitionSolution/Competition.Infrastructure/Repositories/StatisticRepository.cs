using Competition.Application.Common.Models;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class StatisticRepository : Repository<Statistic>, IStatistic
{
    private readonly AppDbContext _db;
    public StatisticRepository(AppDbContext db) : base(db) => _db = db;

    private IQueryable<Statistic> Details() => _db.Statistics
        .Include(x => x.Match)
        .Include(x => x.TeamStatistics).ThenInclude(x => x.Selection);

    public Task<Statistic?> GetDetailsAsync(string publicId, CancellationToken cancellationToken = default) =>
        Details().FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);

    public Task<Statistic?> GetForMatchAsync(string matchPublicId, CancellationToken cancellationToken = default) =>
        Details().FirstOrDefaultAsync(x => x.Match.PublicId == matchPublicId, cancellationToken);

    // Include hidden children of soft-deleted matches in uniqueness checks: the database index does too.
    public Task<bool> ExistsForMatchAsync(Guid matchId, CancellationToken cancellationToken = default) =>
        _db.Statistics.IgnoreQueryFilters().AnyAsync(x => x.MatchId == matchId && x.DeletedAt == null, cancellationToken);

    public const string FinishedStatusSlug = "finished";

    private IQueryable<Match> FinishedMatches(ChartFilter filter)
    {
        var query = _db.Matches.AsNoTracking().Where(x => x.Status != null && x.Status.Slug == FinishedStatusSlug
            && x.HomeScore != null && x.AwayScore != null && x.HomeSelection != null && x.AwaySelection != null);
        if (filter.SeasonPublicId is not null)
            query = query.Where(x => x.Round != null && x.Round.Season!.PublicId == filter.SeasonPublicId);
        if (filter.CompetitionPublicId is not null)
            query = query.Where(x => x.Round != null && x.Round.Season!.Competition!.PublicId == filter.CompetitionPublicId);
        return query;
    }

    public async Task<IReadOnlyList<ChartMatchRow>> GetChartMatchesAsync(ChartFilter filter,
        CancellationToken cancellationToken = default) =>
        await FinishedMatches(filter).Select(x => new ChartMatchRow(
                x.Round != null ? x.Round.Number : null, x.Round != null ? x.Round.Name : null,
                x.HomeSelection!.PublicId, x.HomeSelection.Name, x.HomeScore!.Value,
                x.AwaySelection!.PublicId, x.AwaySelection.Name, x.AwayScore!.Value))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ChartTeamShotRow>> GetChartTeamShotsAsync(ChartFilter filter,
        CancellationToken cancellationToken = default)
    {
        var matchIds = FinishedMatches(filter).Select(x => x.Id);
        return await _db.TeamStatistics.AsNoTracking()
            .Where(x => matchIds.Contains(x.Statistic.MatchId))
            .Select(x => new ChartTeamShotRow(x.Selection.PublicId, x.Selection.Name,
                x.TotalShots, x.ShotsOnTarget, x.Possession))
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<AdvancedMatchRow> ToAdvancedRows(IQueryable<Match> query) => query
        .OrderByDescending(x => x.Date).ThenBy(x => x.PublicId)
        .Select(x => new AdvancedMatchRow(x.Date, x.HomeSelection!.PublicId, x.HomeSelection.Name, x.HomeScore!.Value,
            x.AwaySelection!.PublicId, x.AwaySelection.Name, x.AwayScore!.Value));

    public async Task<IReadOnlyList<AdvancedMatchRow>> GetTeamMatchesAsync(string selectionPublicId, int last, ChartFilter filter,
        CancellationToken cancellationToken = default) =>
        await ToAdvancedRows(FinishedMatches(filter).Where(x =>
                x.HomeSelection!.PublicId == selectionPublicId || x.AwaySelection!.PublicId == selectionPublicId))
            .Take(last).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AdvancedMatchRow>> GetHeadToHeadMatchesAsync(string firstPublicId, string secondPublicId, ChartFilter filter,
        CancellationToken cancellationToken = default) =>
        await ToAdvancedRows(FinishedMatches(filter).Where(x =>
                (x.HomeSelection!.PublicId == firstPublicId && x.AwaySelection!.PublicId == secondPublicId)
                || (x.HomeSelection!.PublicId == secondPublicId && x.AwaySelection!.PublicId == firstPublicId)))
            .ToListAsync(cancellationToken);

    public const string GoalCode = "goal", PenaltyCode = "penalty", YellowCardCode = "yellow-card", RedCardCode = "red-card";

    public async Task<PagedResult<PlayerStatsRow>> GetPlayerStatsAsync(string? selectionPublicId, ChartFilter filter, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var matchIds = FinishedMatches(filter).Select(x => x.Id);
        var events = _db.Events.AsNoTracking().Where(x => x.Player != null && matchIds.Contains(x.MatchId));
        if (selectionPublicId is not null)
            events = events.Where(x => x.Selection.PublicId == selectionPublicId);
        return await events
            .GroupBy(x => new { x.Player!.PublicId, x.Player.FirstName, x.Player.LastName, SelectionName = x.Selection.Name })
            .Select(g => new
            {
                g.Key.PublicId, Name = g.Key.FirstName + " " + g.Key.LastName, g.Key.SelectionName,
                Goals = g.Count(x => x.EventType.Code == GoalCode || x.EventType.Code == PenaltyCode),
                YellowCards = g.Count(x => x.EventType.Code == YellowCardCode),
                RedCards = g.Count(x => x.EventType.Code == RedCardCode),
            })
            .Where(x => x.Goals + x.YellowCards + x.RedCards > 0)
            .OrderByDescending(x => x.Goals).ThenBy(x => x.RedCards + x.YellowCards).ThenBy(x => x.Name).ThenBy(x => x.PublicId)
            .Select(x => new PlayerStatsRow(x.PublicId, x.Name, x.SelectionName, x.Goals, x.YellowCards, x.RedCards))
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<PagedResult<Statistic>> GetFilteredAsync(StatisticFilter filter, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Details().AsNoTracking();
        if (filter.MatchPublicId is not null)
            query = query.Where(x => x.Match.PublicId == filter.MatchPublicId);
        if (filter.SelectionPublicId is not null)
            query = query.Where(x => x.TeamStatistics.Any(t => t.Selection.PublicId == filter.SelectionPublicId));
        if (filter.DateFrom.HasValue)
            query = query.Where(x => x.Match.Date >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue)
            query = query.Where(x => x.Match.Date <= filter.DateTo.Value);
        if (filter.MinTotalShots.HasValue)
            query = query.Where(x => x.TeamStatistics.Any(t => t.TotalShots >= filter.MinTotalShots.Value));
        return await query.OrderByDescending(x => x.Match.Date).ThenBy(x => x.PublicId)
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }
}
