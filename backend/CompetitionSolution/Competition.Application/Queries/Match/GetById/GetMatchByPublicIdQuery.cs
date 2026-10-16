using Competition.Application.DTOs.MatchDto;
using MediatR;

namespace Competition.Application.Queries.Match.GetById;

public class GetMatchByPublicIdQuery : IRequest<MatchDetailsDto>
{
    public string PublicId { get; set; } = string.Empty;
}
