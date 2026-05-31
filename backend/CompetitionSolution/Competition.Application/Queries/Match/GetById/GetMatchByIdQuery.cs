using Competition.Application.DTOs.MatchDto;
using MediatR;

namespace Competition.Application.Queries.Match.GetById;

public class GetMatchByIdQuery : IRequest<MatchDetailsDto>
{
    public Guid Id { get; set; }
}
