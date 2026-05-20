using Competition.Application.Interfaces;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class StadiumRepository : Repository<Stadium>, IStadium
{
    private readonly AppDbContext _context;
    public StadiumRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Stadium?> GetBySlugAsync(string slug)
    {
        return await _context.Stadiums
            .Include(s => s.GeoData)
            .Include(s => s.City)
            .FirstOrDefaultAsync(s => s.Slug == slug);
    }

    public override async Task<IEnumerable<Stadium>> GetAllAsync()
    {
        return await _context.Stadiums
            .Include(s => s.GeoData)
            .Include(s => s.City)
            .ToListAsync();
    }
}
