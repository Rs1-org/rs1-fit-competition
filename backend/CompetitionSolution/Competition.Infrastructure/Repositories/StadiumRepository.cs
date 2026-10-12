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

        return await query
            .OrderBy(s => s.Name)
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }
}
