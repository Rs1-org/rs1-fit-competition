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
        return await _context.Cities.Include(c => c.GeoData).FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
    }

    public async Task<PagedResult<City>> GetPagedByCountryAsync(Guid countryId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Cities
            .Where(c => c.CountryId == countryId)
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<PagedResult<City>> GetPagedFilteredAsync(CityListFilter filter, int page, int pageSize, CancellationToken cancellationToken = default)
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

        return await query
            .OrderBy(c => c.Name)
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }
}