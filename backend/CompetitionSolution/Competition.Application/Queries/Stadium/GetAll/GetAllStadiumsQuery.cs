using Competition.Application.Common.Models;
using Competition.Application.DTOs.StadiumDto;
using MediatR;

namespace Competition.Application.Queries.Stadium.GetAll;

public class GetAllStadiumsQuery : IRequest<PagedResult<StadiumListDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Name { get; init; }
    public string? CountryPublicId { get; init; }
    public string? CityPublicId { get; init; }
    public int? MinCapacity { get; init; }
    public int? MaxCapacity { get; init; }
    public string? SortBy { get; init; }
    public string? SortDir { get; init; }
}
