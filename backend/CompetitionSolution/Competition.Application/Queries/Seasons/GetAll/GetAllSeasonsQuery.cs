using Competition.Application.Common.Models;
using Competition.Application.DTOs.SeasonDto;
using MediatR;

namespace Competition.Application.Queries.Seasons.GetAll;

public class GetAllSeasonsQuery : IRequest<PagedResult<SeasonListDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? CompetitionId { get; init; }
    public bool? IsCurrent { get; init; }
    public string? Search { get; init; }
}
