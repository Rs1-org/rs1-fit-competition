using Competition.Application.Common.Models;
using Competition.Application.DTOs.CityDto;
using MediatR;

namespace Competition.Application.Queries.City.GetAll
{
    public class GetAllCitiesCommand : IRequest<PagedResult<CityListDto>>
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string? Name { get; init; }
        public Guid? CountryId { get; init; }
        public bool? IsCapital { get; init; }
        public int? MinPopulation { get; init; }
        public int? MaxPopulation { get; init; }
    }
}
