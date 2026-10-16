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
            return await Sorted(Filtered(filter), filter.Sort)
                .ToPagedResultAsync(page, pageSize, cancellationToken);
        }

        public async Task<List<Country>> GetFilteredListAsync(CountryListFilter filter, int maxRows, CancellationToken cancellationToken = default)
        {
            return await Sorted(Filtered(filter), filter.Sort)
                .Take(maxRows)
                .ToListAsync(cancellationToken);
        }

        // Whitelisted keys only (validated in the query validators); ThenBy(Id) keeps paging stable.
        private static IQueryable<Country> Sorted(IQueryable<Country> query, SortSpec? sort)
        {
            var desc = sort?.Descending ?? false;
            IOrderedQueryable<Country> ordered = sort is not null && sort.Is("createdAt")
                ? (desc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt))
                : (desc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name));
            return ordered.ThenBy(c => c.Id);
        }

        private IQueryable<Country> Filtered(CountryListFilter filter)
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

            return query;
        }

        private static Instant ToInstant(DateTime date) =>
            Instant.FromDateTimeUtc(DateTime.SpecifyKind(date, DateTimeKind.Utc));
    }
}
