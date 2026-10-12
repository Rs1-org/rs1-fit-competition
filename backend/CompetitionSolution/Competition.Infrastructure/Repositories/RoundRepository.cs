using Competition.Application.Common.Models;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.CompetitionEntities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class RoundRepository : Repository<Round>, IRound
{
    private readonly AppDbContext _context;

    public RoundRepository(AppDbContext context) : base(context)
        => _context = context;

    public async Task<Round?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        await _context.Rounds
            .Include(r => r.Season)
                .ThenInclude(s => s!.Competition)
            .FirstOrDefaultAsync(r => r.Slug == slug, cancellationToken);

    public async Task<PagedResult<Round>> GetPagedWithFiltersAsync(
        int page, int pageSize,
        Guid? seasonId, string? search,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Rounds
            .Include(r => r.Matches)
            .AsQueryable();

        if (seasonId.HasValue)
            query = query.Where(r => r.SeasonId == seasonId.Value);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => EF.Functions.ILike(r.Name, $"%{search.Trim()}%"));

        return await query
            .OrderBy(r => r.Number)
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<bool> NumberExistsInSeasonAsync(Guid seasonId, int number, Guid? excludeId, CancellationToken cancellationToken = default) =>
        await _context.Rounds.AnyAsync(
            r => r.SeasonId == seasonId && r.Number == number && (!excludeId.HasValue || r.Id != excludeId.Value),
            cancellationToken);

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId, CancellationToken cancellationToken = default) =>
        await _context.Rounds.AnyAsync(
            r => r.Slug == slug && (!excludeId.HasValue || r.Id != excludeId.Value),
            cancellationToken);

    public async Task<bool> HasMatchesAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Matches.AnyAsync(m => m.RoundId == id, cancellationToken);
}
