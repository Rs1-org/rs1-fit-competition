using AutoMapper;
using Competition.Application.DTOs.CompetitionDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.Competitions.GetBySlug;

public class GetCompetitionBySlugQueryHandler : IRequestHandler<GetCompetitionBySlugQuery, CompetitionDetailsDto>
{
    private readonly ICompetition _competition;
    private readonly IMapper _mapper;

    public GetCompetitionBySlugQueryHandler(ICompetition competition, IMapper mapper)
    {
        _competition = competition;
        _mapper = mapper;
    }

    public async Task<CompetitionDetailsDto> Handle(GetCompetitionBySlugQuery request, CancellationToken cancellationToken)
    {
        var competition = await _competition.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Competition with slug '{request.Slug}' not found.");
        return _mapper.Map<CompetitionDetailsDto>(competition);
    }
}
