using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class ClubRepository : Repository<Club>, IClubRepository
{
    private readonly AppDbContext _context;

    public ClubRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Club?> GetBySlugAsync(string slug)
    {
        return await GetDetailedQuery()
            .FirstOrDefaultAsync(c => c.Slug == slug);
    }

    public async Task<Club?> GetDetailedByIdAsync(Guid id)
    {
        return await GetDetailedQuery()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public override async Task<IEnumerable<Club>> GetAllAsync()
    {
        return await GetDetailedQuery().ToListAsync();
    }

    private IQueryable<Club> GetDetailedQuery()
    {
        return _context.Clubs
            .Include(c => c.City)
                .ThenInclude(c => c!.Country)
            .Include(c => c.City)
                .ThenInclude(c => c!.GeoData)
            .Include(c => c.Stadium)
                .ThenInclude(s => s!.GeoData)
            .Include(c => c.Stadium)
                .ThenInclude(s => s!.City);
    }
}
