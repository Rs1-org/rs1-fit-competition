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
        Task<Country?> GetBySlugAsync(string slug);
        Task<PagedResult<Country>> GetPagedWithSearchAsync(int page, int pageSize, string? searchTerm, CancellationToken cancellationToken = default);
    }
}
