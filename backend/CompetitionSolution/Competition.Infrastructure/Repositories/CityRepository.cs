using Competition.Application.Common.Models;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class CityRepository : Repository<City>, ICity
{
    private readonly AppDbContext _context;
    public CityRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public new async Task<IEnumerable<City>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Cities.Include(c => c.Country).ToListAsync(cancellationToken);
    }

    public async Task<List<City>> GetAllByCountryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Cities.Include(c => c.Country).Where(s => s.CountryId == id).ToListAsync(cancellationToken);
    }

    public async Task<City?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Cities.Include(c => c.GeoData).Include(c => c.Country).FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
    }

    public async Task<PagedResult<City>> GetPagedByCountryAsync(Guid countryId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Cities
            .Where(c => c.CountryId == countryId)
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<PagedResult<City>> GetPagedFilteredAsync(CityListFilter filter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await Sorted(Filtered(filter), filter.Sort)
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<List<City>> GetFilteredListAsync(CityListFilter filter, int maxRows, CancellationToken cancellationToken = default)
    {
        return await Sorted(Filtered(filter).Include(c => c.GeoData), filter.Sort)
            .Take(maxRows)
            .ToListAsync(cancellationToken);
    }

    // Whitelisted keys only (validated in the query validators); ThenBy(Id) keeps paging stable.
    private static IQueryable<City> Sorted(IQueryable<City> query, SortSpec? sort)
    {
        var desc = sort?.Descending ?? false;
        IOrderedQueryable<City> ordered = sort switch
        {
            _ when sort is not null && sort.Is("population") =>
                desc ? query.OrderByDescending(c => c.Population) : query.OrderBy(c => c.Population),
            _ when sort is not null && sort.Is("country") =>
                desc ? query.OrderByDescending(c => c.Country!.Name).ThenBy(c => c.Name)
                     : query.OrderBy(c => c.Country!.Name).ThenBy(c => c.Name),
            _ => desc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
        };
        return ordered.ThenBy(c => c.Id);
    }

    private IQueryable<City> Filtered(CityListFilter filter)
    {
        var query = _context.Cities.Include(c => c.Country).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var name = filter.Name.Trim().ToLower();
            query = query.Where(c => c.Name!.ToLower().Contains(name));
        }
        if (filter.CountryId.HasValue)
            query = query.Where(c => c.CountryId == filter.CountryId.Value);
        if (filter.IsCapital.HasValue)
            query = query.Where(c => c.IsCapital == filter.IsCapital.Value);
        if (filter.MinPopulation.HasValue)
            query = query.Where(c => c.Population >= filter.MinPopulation.Value);
        if (filter.MaxPopulation.HasValue)
            query = query.Where(c => c.Population <= filter.MaxPopulation.Value);

        return query;
    }
}