using Competition.Application.Common.Models;
using Competition.Application.DTOs.MatchDto;
using MediatR;

namespace Competition.Application.Queries.Match.GetAll;

public class GetAllMatchesQuery : IRequest<PagedResult<MatchListDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? StatusId { get; init; }
    public Guid? StadiumId { get; init; }
    public Guid? RoundId { get; init; }
    public bool? IsPostponed { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
}
