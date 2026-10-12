using Competition.Application.DTOs.SeasonDto;
using MediatR;

namespace Competition.Application.Queries.Seasons.GetBySlug;

public class GetSeasonBySlugQuery : IRequest<SeasonDetailsDto>
{
    public string Slug { get; init; } = string.Empty;
}
