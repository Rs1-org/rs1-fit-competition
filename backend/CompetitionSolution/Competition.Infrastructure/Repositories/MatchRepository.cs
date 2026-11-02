using Competition.Application.Common.Models;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Competition.Infrastructure.Repositories;

public class MatchRepository : Repository<Match>, IMatch
{
    private readonly AppDbContext _context;

    public MatchRepository(AppDbContext context) : base(context)
        => _context = context;

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default) =>
        await _context.Matches.IgnoreQueryFilters().AnyAsync(m => m.Slug == slug, cancellationToken);

    public async Task<Match?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        await _context.Matches
            .Include(m => m.Status)
            .Include(m => m.Stadium)
            .Include(m => m.HomeSelection)
            .Include(m => m.AwaySelection)
            .Include(m => m.Round)
                .ThenInclude(r => r!.Season)
                    .ThenInclude(s => s!.Competition)
            .FirstOrDefaultAsync(m => m.Slug == slug, cancellationToken);

    public async Task<Match?> GetByIdWithNavAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Matches
            .Include(m => m.Status)
            .Include(m => m.Stadium)
            .Include(m => m.HomeSelection)
            .Include(m => m.AwaySelection)
            .Include(m => m.Round)
                .ThenInclude(r => r!.Season)
                    .ThenInclude(s => s!.Competition)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<PagedResult<Match>> GetPagedWithFiltersAsync(int page, int pageSize, MatchListFilter filter, CancellationToken cancellationToken = default) =>
        await Sorted(Filtered(filter), filter.Sort)
            .ToPagedResultAsync(page, pageSize, cancellationToken);

    public async Task<List<Match>> GetFilteredListAsync(MatchListFilter filter, int maxRows, CancellationToken cancellationToken = default) =>
        await Sorted(Filtered(filter), filter.Sort)
            .Take(maxRows)
            .ToListAsync(cancellationToken);

    private IQueryable<Match> Filtered(MatchListFilter filter)
    {
        var query = _context.Matches
            .Include(m => m.Status)
            .Include(m => m.Stadium)
            .Include(m => m.HomeSelection)
            .Include(m => m.AwaySelection)
            .Include(m => m.Round)
                .ThenInclude(r => r!.Season)
                    .ThenInclude(s => s!.Competition)
            .AsQueryable();

        if (filter.StatusId.HasValue)
            query = query.Where(m => m.StatusId == filter.StatusId.Value);

        if (filter.StadiumId.HasValue)
            query = query.Where(m => m.StadiumId == filter.StadiumId.Value);
        if (filter.RoundId.HasValue)
            query = query.Where(m => m.RoundId == filter.RoundId.Value);

        if (filter.IsPostponed.HasValue)
            query = query.Where(m => m.IsPostponed == filter.IsPostponed.Value);

        if (filter.DateFrom.HasValue)
        {
            var from = Instant.FromDateTimeUtc(DateTime.SpecifyKind(filter.DateFrom.Value, DateTimeKind.Utc));
            query = query.Where(m => m.Date >= from);
        }

        if (filter.DateTo.HasValue)
        {
            var to = Instant.FromDateTimeUtc(DateTime.SpecifyKind(filter.DateTo.Value, DateTimeKind.Utc));
            query = query.Where(m => m.Date <= to);
        }

        if (filter.SelectionId.HasValue)
            query = query.Where(m => m.HomeSelectionId == filter.SelectionId.Value || m.AwaySelectionId == filter.SelectionId.Value);

        if (filter.CompetitionId.HasValue)
            query = query.Where(m => m.Round != null && m.Round.Season != null && m.Round.Season.CompetitionId == filter.CompetitionId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Phase))
        {
            var statusNames = MatchPhases.StatusNames(filter.Phase);
            query = query.Where(m => !m.IsPostponed && statusNames.Contains(m.Status!.Name.ToLower()));
        }

        return query;
    }

    // Whitelisted keys only (validated in the query validators); ThenBy(Id) keeps paging stable.
    private static IQueryable<Match> Sorted(IQueryable<Match> query, SortSpec? sort)
    {
        var desc = sort?.Descending ?? false;
        IOrderedQueryable<Match> ordered = sort switch
        {
            _ when sort is not null && sort.Is("home") =>
                desc ? query.OrderByDescending(m => m.HomeSelection!.Name) : query.OrderBy(m => m.HomeSelection!.Name),
            _ when sort is not null && sort.Is("away") =>
                desc ? query.OrderByDescending(m => m.AwaySelection!.Name) : query.OrderBy(m => m.AwaySelection!.Name),
            _ => desc ? query.OrderByDescending(m => m.Date) : query.OrderBy(m => m.Date)
        };
        return ordered.ThenBy(m => m.Id);
    }
}
