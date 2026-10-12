using AutoMapper;
using Competition.Application.DTOs.RoundDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.Rounds.GetBySlug;

public class GetRoundBySlugQueryHandler : IRequestHandler<GetRoundBySlugQuery, RoundDetailsDto>
{
    private readonly IRound _round;
    private readonly IMapper _mapper;

    public GetRoundBySlugQueryHandler(IRound round, IMapper mapper)
    {
        _round = round;
        _mapper = mapper;
    }

    public async Task<RoundDetailsDto> Handle(GetRoundBySlugQuery request, CancellationToken cancellationToken)
    {
        var round = await _round.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Round with slug '{request.Slug}' not found.");
        return _mapper.Map<RoundDetailsDto>(round);
    }
}
