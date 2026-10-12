using Competition.Application.Common.Models;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.CompetitionEntities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class SeasonRepository : Repository<Season>, ISeason
{
    private readonly AppDbContext _context;

    public SeasonRepository(AppDbContext context) : base(context)
        => _context = context;

    public async Task<Season?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        await _context.Seasons
            .Include(s => s.Competition)
            .FirstOrDefaultAsync(s => s.Slug == slug, cancellationToken);

    public async Task<PagedResult<Season>> GetPagedWithFiltersAsync(
        int page, int pageSize,
        Guid? competitionId, bool? isCurrent, string? search,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Seasons
            .Include(s => s.Competition)
            .Include(s => s.Rounds)
            .AsQueryable();

        if (competitionId.HasValue)
            query = query.Where(s => s.CompetitionId == competitionId.Value);
        if (isCurrent.HasValue)
            query = query.Where(s => s.IsCurrent == isCurrent.Value);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => EF.Functions.ILike(s.Name, $"%{search.Trim()}%"));

        return await query
            .OrderByDescending(s => s.StartDate)
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    // The slug embeds the competition slug, so this also covers "same name in the same competition".
    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId, CancellationToken cancellationToken = default) =>
        await _context.Seasons.AnyAsync(
            s => s.Slug == slug && (!excludeId.HasValue || s.Id != excludeId.Value),
            cancellationToken);

    public async Task<bool> HasRoundsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Rounds.AnyAsync(r => r.SeasonId == id, cancellationToken);

    public async Task UnsetCurrentAsync(Guid competitionId, Guid? exceptId, CancellationToken cancellationToken = default)
    {
        // Tracked update (not ExecuteUpdate) so ModifiedAt is stamped by SaveChanges.
        var current = await _context.Seasons
            .Where(s => s.CompetitionId == competitionId && s.IsCurrent && (!exceptId.HasValue || s.Id != exceptId.Value))
            .ToListAsync(cancellationToken);
        foreach (var season in current)
            season.IsCurrent = false;
    }
}
