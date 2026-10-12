using Competition.Application.DTOs.RoundDto;
using MediatR;

namespace Competition.Application.Queries.Rounds.GetBySlug;

public class GetRoundBySlugQuery : IRequest<RoundDetailsDto>
{
    public string Slug { get; init; } = string.Empty;
}
