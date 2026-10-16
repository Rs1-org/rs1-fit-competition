using Competition.Application.Common.Models;
using Competition.Application.DTOs.CompetitionDto;
using MediatR;

namespace Competition.Application.Queries.Competitions.GetAll;

public class GetAllCompetitionsQuery : IRequest<PagedResult<CompetitionListDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
    public string? CountryPublicId { get; init; }
}
