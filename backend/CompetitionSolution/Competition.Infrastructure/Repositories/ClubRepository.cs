using Competition.Application.Common.Models;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class ClubRepository(AppDbContext context) : Repository<Club>(context), IClub
{
    private IQueryable<Club> WithDetails() => context.Clubs
        .Include(c => c.City).ThenInclude(c => c!.Country)
        .Include(c => c.Stadium);

    public Task<Club?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        WithDetails().FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, Guid? excludeId, CancellationToken cancellationToken = default) =>
        context.Clubs.AnyAsync(c => c.Slug == slug && (!excludeId.HasValue || c.Id != excludeId.Value), cancellationToken);

    public Task<PagedResult<Club>> GetPagedFilteredAsync(ClubListFilter filter, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = WithDetails().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var term = filter.Name.Trim().ToLowerInvariant();
            query = query.Where(c => c.Name.ToLower().Contains(term));
        }
        if (filter.CountryId.HasValue)
            query = query.Where(c => c.City != null && c.City.Country != null && c.City.CountryId == filter.CountryId);
        if (filter.CityId.HasValue)
            query = query.Where(c => c.City != null && c.CityId == filter.CityId);
        if (filter.FoundedFrom.HasValue) query = query.Where(c => c.FoundingDate >= filter.FoundedFrom);
        if (filter.FoundedTo.HasValue) query = query.Where(c => c.FoundingDate <= filter.FoundedTo);
        if (filter.HasStadium.HasValue) query = query.Where(c => (c.Stadium != null) == filter.HasStadium);
        if (filter.ClubAge.HasValue) query = query.Where(c => c.ClubAge == filter.ClubAge);

        var sorted = (filter.SortBy, filter.Descending) switch
        {
            ("foundingDate", false) => query.OrderBy(c => c.FoundingDate),
            ("foundingDate", true) => query.OrderByDescending(c => c.FoundingDate),
            ("clubAge", false) => query.OrderBy(c => c.ClubAge),
            ("clubAge", true) => query.OrderByDescending(c => c.ClubAge),
            (_, true) => query.OrderByDescending(c => c.Name),
            _ => query.OrderBy(c => c.Name)
        };
        return sorted.ThenBy(c => c.PublicId).ToPagedResultAsync(page, pageSize, cancellationToken);
    }
}
