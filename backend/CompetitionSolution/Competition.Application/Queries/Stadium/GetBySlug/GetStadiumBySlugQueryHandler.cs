using AutoMapper;
using Competition.Application.DTOs.StadiumDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.Stadium.GetBySlug;

public class GetStadiumBySlugQueryHandler : IRequestHandler<GetStadiumBySlugQuery, StadiumDetailsDto>
{
    private readonly IStadium _stadium;
    private readonly IMapper _mapper;

    public GetStadiumBySlugQueryHandler(IStadium stadium, IMapper mapper)
    {
        _stadium = stadium;
        _mapper = mapper;
    }

    public async Task<StadiumDetailsDto> Handle(GetStadiumBySlugQuery request, CancellationToken cancellationToken)
    {
        var stadium = await _stadium.GetBySlugAsync(request.Slug!, cancellationToken);
        if (stadium == null)
        {
            throw new KeyNotFoundException($"Couldn't find any stadium with slug {request.Slug}");
        }

        return _mapper.Map<StadiumDetailsDto>(stadium);
    }
}
