using Competition.Application.DTOs.CityDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories;

public class CityRepository : Repository<City>, ICity
{
    private readonly AppDbContext _context;
    public CityRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<City>> GetAllByCountryAsync(Guid id)
    {
        return await _context.Cities.Where(s => s.CountryId == id).ToListAsync();
    }

    public async Task<City?> GetBySlugAsync(string slug)
    {
        return await _context.Cities.FirstOrDefaultAsync(c => c.Slug == slug);
    }
}