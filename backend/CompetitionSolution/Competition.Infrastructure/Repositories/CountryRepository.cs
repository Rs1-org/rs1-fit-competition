using Competition.Application.Common.Models;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
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

        public async Task<Country?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _context.Countries.FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
        }

        public async Task<PagedResult<Country>> GetPagedFilteredAsync(CountryListFilter filter, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.Countries.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                var name = filter.Name.Trim().ToLower();
                query = query.Where(c => c.Name!.ToLower().Contains(name));
            }
            if (!string.IsNullOrWhiteSpace(filter.Slug))
            {
                var slug = filter.Slug.Trim().ToLower();
                query = query.Where(c => c.Slug!.ToLower().Contains(slug));
            }
            if (filter.HasPicture.HasValue)
                query = filter.HasPicture.Value
                    ? query.Where(c => c.PictureUrl != null && c.PictureUrl != "")
                    : query.Where(c => c.PictureUrl == null || c.PictureUrl == "");
            if (filter.CreatedFrom.HasValue)
            {
                var from = ToInstant(filter.CreatedFrom.Value.Date);
                query = query.Where(c => c.CreatedAt >= from);
            }
            if (filter.CreatedTo.HasValue)
            {
                // Inclusive end date: everything before the start of the next day.
                var to = ToInstant(filter.CreatedTo.Value.Date.AddDays(1));
                query = query.Where(c => c.CreatedAt < to);
            }

            return await query
                .OrderBy(c => c.Name)
                .ToPagedResultAsync(page, pageSize, cancellationToken);
        }

        private static Instant ToInstant(DateTime date) =>
            Instant.FromDateTimeUtc(DateTime.SpecifyKind(date, DateTimeKind.Utc));
    }
}
