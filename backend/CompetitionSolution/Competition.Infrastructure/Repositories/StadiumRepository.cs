using Competition.Application.Common.Models;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class StadiumRepository : Repository<Stadium>, IStadium
{
    private readonly AppDbContext _context;
    public StadiumRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Stadium?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Stadiums
            .Include(s => s.GeoData)
            .Include(s => s.City)
                .ThenInclude(c => c!.GeoData)
            .FirstOrDefaultAsync(s => s.Slug == slug, cancellationToken);
    }

    public override async Task<IEnumerable<Stadium>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Stadiums
            .Include(s => s.GeoData)
            .Include(s => s.City)
                .ThenInclude(c => c!.Country)
            .ToListAsync(cancellationToken);
    }

    public override async Task<PagedResult<Stadium>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Stadiums
            .Include(s => s.GeoData)
            .Include(s => s.City)
                .ThenInclude(c => c!.Country)
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<PagedResult<Stadium>> GetPagedFilteredAsync(StadiumListFilter filter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await Sorted(Filtered(filter), filter.Sort)
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<List<Stadium>> GetFilteredListAsync(StadiumListFilter filter, int maxRows, CancellationToken cancellationToken = default)
    {
        return await Sorted(Filtered(filter).Include(s => s.GeoData), filter.Sort)
            .Take(maxRows)
            .ToListAsync(cancellationToken);
    }

    // Whitelisted keys only (validated in the query validators); ThenBy(Id) keeps paging stable.
    private static IQueryable<Stadium> Sorted(IQueryable<Stadium> query, SortSpec? sort)
    {
        var desc = sort?.Descending ?? false;
        IOrderedQueryable<Stadium> ordered = sort switch
        {
            _ when sort is not null && sort.Is("capacity") =>
                desc ? query.OrderByDescending(s => s.Capacity) : query.OrderBy(s => s.Capacity),
            _ when sort is not null && sort.Is("city") =>
                desc ? query.OrderByDescending(s => s.City!.Name).ThenBy(s => s.Name)
                     : query.OrderBy(s => s.City!.Name).ThenBy(s => s.Name),
            _ when sort is not null && sort.Is("dateOfConstruction") =>
                desc ? query.OrderByDescending(s => s.DateOfConstruction) : query.OrderBy(s => s.DateOfConstruction),
            _ => desc ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
        };
        return ordered.ThenBy(s => s.Id);
    }

    private IQueryable<Stadium> Filtered(StadiumListFilter filter)
    {
        var query = _context.Stadiums
            .Include(s => s.City)
                .ThenInclude(c => c!.Country)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var name = filter.Name.Trim().ToLower();
            query = query.Where(s => s.Name!.ToLower().Contains(name));
        }
        if (filter.CountryId.HasValue)
            query = query.Where(s => s.City!.CountryId == filter.CountryId.Value);
        if (filter.CityId.HasValue)
            query = query.Where(s => s.CityId == filter.CityId.Value);
        if (filter.MinCapacity.HasValue)
            query = query.Where(s => s.Capacity >= filter.MinCapacity.Value);
        if (filter.MaxCapacity.HasValue)
            query = query.Where(s => s.Capacity <= filter.MaxCapacity.Value);

        return query;
    }
}
