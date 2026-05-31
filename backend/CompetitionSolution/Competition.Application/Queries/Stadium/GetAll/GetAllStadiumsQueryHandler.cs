using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.StadiumDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.Stadium.GetAll;

public class GetAllStadiumsQueryHandler : IRequestHandler<GetAllStadiumsQuery, PagedResult<StadiumListDto>>
{
    private readonly IStadium _stadium;
    private readonly IMapper _mapper;

    public GetAllStadiumsQueryHandler(IStadium stadium, IMapper mapper)
    {
        _stadium = stadium;
        _mapper = mapper;
    }

    public async Task<PagedResult<StadiumListDto>> Handle(GetAllStadiumsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _stadium.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
        var items = _mapper.Map<IReadOnlyList<StadiumListDto>>(paged.Items);
        return new PagedResult<StadiumListDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
