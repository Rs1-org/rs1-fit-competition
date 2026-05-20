using AutoMapper;
using Competition.Application.DTOs.StadiumDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.Stadium.GetAll;

public class GetAllStadiumsQueryHandler : IRequestHandler<GetAllStadiumsQuery, IEnumerable<StadiumListDto>>
{
    private readonly IStadium _stadium;
    private readonly IMapper _mapper;

    public GetAllStadiumsQueryHandler(IStadium stadium, IMapper mapper)
    {
        _stadium = stadium;
        _mapper = mapper;
    }

    public async Task<IEnumerable<StadiumListDto>> Handle(GetAllStadiumsQuery request, CancellationToken cancellationToken)
    {
        var stadiums = await _stadium.GetAllAsync();
        return _mapper.Map<IEnumerable<StadiumListDto>>(stadiums);
    }
}
