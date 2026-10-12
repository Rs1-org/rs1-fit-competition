using AutoMapper;
using Competition.Application.DTOs.SeasonDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.Seasons.GetBySlug;

public class GetSeasonBySlugQueryHandler : IRequestHandler<GetSeasonBySlugQuery, SeasonDetailsDto>
{
    private readonly ISeason _season;
    private readonly IMapper _mapper;

    public GetSeasonBySlugQueryHandler(ISeason season, IMapper mapper)
    {
        _season = season;
        _mapper = mapper;
    }

    public async Task<SeasonDetailsDto> Handle(GetSeasonBySlugQuery request, CancellationToken cancellationToken)
    {
        var season = await _season.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException($"Season with slug '{request.Slug}' not found.");
        return _mapper.Map<SeasonDetailsDto>(season);
    }
}
