using Competition.Application.Common.Models;
using Competition.Application.Interfaces;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using CompetitionEntity = Competition.Domain.Entities.CompetitionEntities.Competition;

namespace Competition.Infrastructure.Repositories;

public class CompetitionRepository : Repository<CompetitionEntity>, ICompetition
{
    private readonly AppDbContext _context;

    public CompetitionRepository(AppDbContext context) : base(context)
        => _context = context;

    public async Task<CompetitionEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        await _context.Competitions
            .Include(c => c.Country)
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);

    public async Task<PagedResult<CompetitionEntity>> GetPagedWithFiltersAsync(
        int page, int pageSize,
        string? search, Guid? countryId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Competitions
            .Include(c => c.Country)
            .Include(c => c.Seasons)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => EF.Functions.ILike(c.Name, $"%{search.Trim()}%")
                || (c.ShortName != null && EF.Functions.ILike(c.ShortName, $"%{search.Trim()}%")));
        if (countryId.HasValue)
            query = query.Where(c => c.CountryId == countryId.Value);

        return await query
            .OrderBy(c => c.Name)
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    // Slug, not name: "Liga 1" and "Liga-1" differ as names but share a slug (unique index).
    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId, CancellationToken cancellationToken = default) =>
        await _context.Competitions.AnyAsync(
            c => c.Slug == slug && (!excludeId.HasValue || c.Id != excludeId.Value),
            cancellationToken);

    public async Task<bool> HasSeasonsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Seasons.AnyAsync(s => s.CompetitionId == id, cancellationToken);
}
