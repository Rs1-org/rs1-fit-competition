using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Repositories
{
    public class CountryRepository : Repository<Country>,ICountry
    {
        private readonly AppDbContext _context;
        public CountryRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Country?> GetBySlugAsync(string slug)
        {
            return await _context.Countries.FirstOrDefaultAsync(c => c.Slug == slug);
        }
    }
}
