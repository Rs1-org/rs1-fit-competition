using Competition.Application.Common.Models;
using Competition.Application.DTOs.MatchDto;
using MediatR;

namespace Competition.Application.Queries.Match.GetAll;

public class GetAllMatchesQuery : IRequest<PagedResult<MatchListDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? StatusPublicId { get; init; }
    public string? StadiumPublicId { get; init; }
    public string? RoundPublicId { get; init; }
    public bool? IsPostponed { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public string? SelectionPublicId { get; init; }
    public string? CompetitionPublicId { get; init; }
    public string? Phase { get; init; }
    public string? SortBy { get; init; }
    public string? SortDir { get; init; }
}
