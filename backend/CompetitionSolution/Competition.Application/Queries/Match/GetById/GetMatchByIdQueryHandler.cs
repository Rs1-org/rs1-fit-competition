using AutoMapper;
using Competition.Application.DTOs.MatchDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.Match.GetById;

public class GetMatchByIdQueryHandler : IRequestHandler<GetMatchByIdQuery, MatchDetailsDto>
{
    private readonly IMatch _match;
    private readonly IMapper _mapper;

    public GetMatchByIdQueryHandler(IMatch match, IMapper mapper)
    {
        _match = match;
        _mapper = mapper;
    }

    public async Task<MatchDetailsDto> Handle(GetMatchByIdQuery request, CancellationToken cancellationToken)
    {
        var match = await _match.GetByIdWithNavAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Match with ID '{request.Id}' not found.");

        return _mapper.Map<MatchDetailsDto>(match);
    }
}
