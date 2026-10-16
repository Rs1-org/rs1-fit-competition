using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.SeasonDto;
using Competition.Application.Interfaces;
using CompetitionEntity = Competition.Domain.Entities.CompetitionEntities.Competition;
using MediatR;

namespace Competition.Application.Queries.Seasons.GetAll;

public class GetAllSeasonsQueryHandler : IRequestHandler<GetAllSeasonsQuery, PagedResult<SeasonListDto>>
{
    private readonly ISeason _season;
    private readonly IPublicIdLookup _lookup;
    private readonly IMapper _mapper;

    public GetAllSeasonsQueryHandler(ISeason season, IPublicIdLookup lookup, IMapper mapper)
    {
        _lookup = lookup;
        _season = season;
        _mapper = mapper;
    }

    public async Task<PagedResult<SeasonListDto>> Handle(GetAllSeasonsQuery request, CancellationToken cancellationToken)
    {
        // Filter arrives as a PublicId; an unknown one yields an empty page, not a 404.
        var competitionId = await _lookup.GetIdByPublicIdAsync<CompetitionEntity>(request.CompetitionPublicId, cancellationToken);
        if (request.CompetitionPublicId is not null && competitionId is null)
            return new PagedResult<SeasonListDto>(Array.Empty<SeasonListDto>(), 0, request.Page, request.PageSize);

        var paged = await _season.GetPagedWithFiltersAsync(
            request.Page, request.PageSize,
            competitionId, request.IsCurrent, request.Search,
            cancellationToken);

        var items = _mapper.Map<IReadOnlyList<SeasonListDto>>(paged.Items);
        return new PagedResult<SeasonListDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
