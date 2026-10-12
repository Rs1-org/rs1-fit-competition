using Competition.Application.Common.Models;
using Competition.Application.DTOs.RoundDto;
using MediatR;

namespace Competition.Application.Queries.Rounds.GetAll;

public class GetAllRoundsQuery : IRequest<PagedResult<RoundListDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? SeasonId { get; init; }
    public string? Search { get; init; }
}
