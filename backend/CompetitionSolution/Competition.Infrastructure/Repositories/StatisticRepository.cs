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
