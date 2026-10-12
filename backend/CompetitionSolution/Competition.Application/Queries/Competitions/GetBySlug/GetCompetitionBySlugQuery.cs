using Competition.Application.DTOs.CompetitionDto;
using MediatR;

namespace Competition.Application.Queries.Competitions.GetBySlug;

public class GetCompetitionBySlugQuery : IRequest<CompetitionDetailsDto>
{
    public string Slug { get; init; } = string.Empty;
}
