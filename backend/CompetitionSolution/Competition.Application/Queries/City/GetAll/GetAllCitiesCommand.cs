using Competition.Application.Common.Models;
using Competition.Application.DTOs.CityDto;
using MediatR;

namespace Competition.Application.Queries.City.GetAll
{
    public class GetAllCitiesCommand : IRequest<PagedResult<CityListDto>>
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string? SearchTerm { get; init; }
    }
}
