using Competition.Application.Common.Models;
using Competition.Application.DTOs.CityDto;
using MediatR;

namespace Competition.Application.Queries.City.GetAllByCountry
{
    public class GetAllCitiesByCountryCommand : IRequest<PagedResult<CityListDto>>
    {
        public Guid CountryId { get; set; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}
