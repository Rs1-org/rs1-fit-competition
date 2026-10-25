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

    public async Task<PagedResult<Match>> GetPagedWithFiltersAsync(
        int page, int pageSize,
        Guid? statusId, Guid? stadiumId, Guid? roundId,
        bool? isPostponed,
        DateTime? dateFrom, DateTime? dateTo,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Matches
            .Include(m => m.Status)
            .Include(m => m.Stadium)
            .Include(m => m.HomeSelection)
            .Include(m => m.AwaySelection)
            .Include(m => m.Round)
            .AsQueryable();

        if (statusId.HasValue)
            query = query.Where(m => m.StatusId == statusId.Value);

        if (stadiumId.HasValue)
            query = query.Where(m => m.StadiumId == stadiumId.Value);
        if (roundId.HasValue)
            query = query.Where(m => m.RoundId == roundId.Value);

        if (isPostponed.HasValue)
            query = query.Where(m => m.IsPostponed == isPostponed.Value);

        if (dateFrom.HasValue)
            query = query.Where(m => m.Date >= Instant.FromDateTimeUtc(
                DateTime.SpecifyKind(dateFrom.Value, DateTimeKind.Utc)));

        if (dateTo.HasValue)
            query = query.Where(m => m.Date <= Instant.FromDateTimeUtc(
                DateTime.SpecifyKind(dateTo.Value, DateTimeKind.Utc)));

        return await query
            .OrderBy(m => m.Date)
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }
}
