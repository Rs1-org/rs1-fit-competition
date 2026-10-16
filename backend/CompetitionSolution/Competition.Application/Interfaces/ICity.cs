using Competition.Application.Common.Models;
using Competition.Application.DTOs.CityDto;
using Competition.Domain.Entities;

namespace Competition.Application.Interfaces;

public interface ICity : IRepository<City>
{
    Task<City?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<City>> GetAllByCountryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<City>> GetPagedByCountryAsync(Guid countryId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<City>> GetPagedFilteredAsync(CityListFilter filter, int page, int pageSize, CancellationToken cancellationToken = default);
    /// <summary>Filtered + sorted, unpaged (export), with Country and GeoData, capped at <paramref name="maxRows"/>.</summary>
    Task<List<City>> GetFilteredListAsync(CityListFilter filter, int maxRows, CancellationToken cancellationToken = default);
}