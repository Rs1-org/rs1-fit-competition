using Competition.Application.Common.Models;
using Competition.Application.DTOs.StadiumDto;
using MediatR;

namespace Competition.Application.Queries.Stadium.GetAll;

public class GetAllStadiumsQuery : IRequest<PagedResult<StadiumListDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
