using Competition.Application.DTOs.MatchDto;
using MediatR;

namespace Competition.Application.Queries.Match.GetBySlug;

public class GetMatchBySlugQuery : IRequest<MatchDetailsDto>
{
    public string? Slug { get; set; }
}
