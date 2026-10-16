using AutoMapper;
using Competition.Application.DTOs.MatchDto;
using Competition.Application.Interfaces;
using MediatR;
using MatchEntity = Competition.Domain.Entities.MatchEntities.Match;

namespace Competition.Application.Queries.Match.GetById;

public class GetMatchByPublicIdQueryHandler : IRequestHandler<GetMatchByPublicIdQuery, MatchDetailsDto>
{
    private readonly IMatch _match;
    private readonly IPublicIdLookup _lookup;
    private readonly IMapper _mapper;

    public GetMatchByPublicIdQueryHandler(IMatch match, IPublicIdLookup lookup, IMapper mapper)
    {
        _match = match;
        _lookup = lookup;
        _mapper = mapper;
    }

    public async Task<MatchDetailsDto> Handle(GetMatchByPublicIdQuery request, CancellationToken cancellationToken)
    {
        var id = await _lookup.GetRequiredIdAsync<MatchEntity>(request.PublicId, cancellationToken);
        var match = await _match.GetByIdWithNavAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Match with public id '{request.PublicId}' not found.");
        return _mapper.Map<MatchDetailsDto>(match);
    }
}
