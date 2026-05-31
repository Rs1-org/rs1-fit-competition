using AutoMapper;
using Competition.Application.DTOs.MatchDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.Match.GetBySlug;

public class GetMatchBySlugQueryHandler : IRequestHandler<GetMatchBySlugQuery, MatchDetailsDto>
{
    private readonly IMatch _match;
    private readonly IMapper _mapper;

    public GetMatchBySlugQueryHandler(IMatch match, IMapper mapper)
    {
        _match = match;
        _mapper = mapper;
    }

    public async Task<MatchDetailsDto> Handle(GetMatchBySlugQuery request, CancellationToken cancellationToken)
    {
        var match = await _match.GetBySlugAsync(request.Slug!, cancellationToken)
            ?? throw new KeyNotFoundException($"Match with slug '{request.Slug}' not found.");

        return _mapper.Map<MatchDetailsDto>(match);
    }
}
