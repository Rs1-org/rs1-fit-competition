using Competition.Application.Common.Models;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<PagedResult<Country>> GetPagedWithSearchAsync(int page, int pageSize, string? searchTerm, CancellationToken cancellationToken = default)
        {
            var query = _context.Countries.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(c => EF.Functions.ILike(c.Name!, $"%{searchTerm}%"));
            return await query.ToPagedResultAsync(page, pageSize, cancellationToken);
        }
    }
}
