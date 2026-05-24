using Competition.Application.Common.Models;
using Competition.Application.DTOs.CountryDto;
using MediatR;

namespace Competition.Application.Queries.City.GetAll
{
    public class GetAllCountriesCommand : IRequest<PagedResult<CountryListDto>>
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}
