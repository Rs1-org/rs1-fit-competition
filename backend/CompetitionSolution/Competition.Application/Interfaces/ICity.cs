using Competition.Application.Common.Models;
using Competition.Application.DTOs.CityDto;
using Competition.Domain.Entities;

namespace Competition.Application.Interfaces;

public interface ICity : IRepository<City>
{
    Task<City?> GetBySlugAsync(string slug);
    Task<List<City>> GetAllByCountryAsync(Guid id);
    Task<PagedResult<City>> GetPagedByCountryAsync(Guid countryId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<City>> GetPagedWithSearchAsync(int page, int pageSize, string? searchTerm, CancellationToken cancellationToken = default);
}