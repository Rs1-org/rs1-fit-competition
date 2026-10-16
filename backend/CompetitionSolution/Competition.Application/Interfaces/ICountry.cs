using Competition.Application.Common.Models;
using Competition.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Interfaces
{
    public interface ICountry : IRepository<Country>
    {
        Task<Country?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<PagedResult<Country>> GetPagedFilteredAsync(CountryListFilter filter, int page, int pageSize, CancellationToken cancellationToken = default);
        /// <summary>Filtered + sorted, unpaged (export), capped at <paramref name="maxRows"/>.</summary>
        Task<List<Country>> GetFilteredListAsync(CountryListFilter filter, int maxRows, CancellationToken cancellationToken = default);
    }
}
