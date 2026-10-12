using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.SeasonDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.Seasons.GetAll;

public class GetAllSeasonsQueryHandler : IRequestHandler<GetAllSeasonsQuery, PagedResult<SeasonListDto>>
{
    private readonly ISeason _season;
    private readonly IMapper _mapper;

    public GetAllSeasonsQueryHandler(ISeason season, IMapper mapper)
    {
        _season = season;
        _mapper = mapper;
    }

    public async Task<PagedResult<SeasonListDto>> Handle(GetAllSeasonsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _season.GetPagedWithFiltersAsync(
            request.Page, request.PageSize,
            request.CompetitionId, request.IsCurrent, request.Search,
            cancellationToken);

        var items = _mapper.Map<IReadOnlyList<SeasonListDto>>(paged.Items);
        return new PagedResult<SeasonListDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
