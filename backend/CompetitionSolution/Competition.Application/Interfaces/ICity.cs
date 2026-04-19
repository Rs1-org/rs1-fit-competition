using Competition.Application.DTOs.CityDto;
using Competition.Domain.Entities;

namespace Competition.Application.Interfaces;

public interface ICity : IRepository<City>
{
    Task<City?> GetBySlugAsync(string slug);
    Task<List<City>> GetAllByCountryAsync(Guid id);

}