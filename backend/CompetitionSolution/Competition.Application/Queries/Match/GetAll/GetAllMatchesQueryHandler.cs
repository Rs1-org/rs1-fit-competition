using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.MatchDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Domain.Entities.CompetitionEntities;
using Competition.Domain.Entities.MatchEntities;
using CompetitionEntity = Competition.Domain.Entities.CompetitionEntities.Competition;
using StadiumEntity = Competition.Domain.Entities.MatchEntities.Stadium;
using MediatR;

namespace Competition.Application.Queries.Match.GetAll;

public class GetAllMatchesQueryHandler : IRequestHandler<GetAllMatchesQuery, PagedResult<MatchListDto>>
{
    private readonly IMatch _match;
    private readonly IPublicIdLookup _lookup;
    private readonly IMapper _mapper;

    public GetAllMatchesQueryHandler(IMatch match, IPublicIdLookup lookup, IMapper mapper)
    {
        _lookup = lookup;
        _match = match;
        _mapper = mapper;
    }

    public async Task<PagedResult<MatchListDto>> Handle(GetAllMatchesQuery request, CancellationToken cancellationToken)
    {
        // Filters arrive as PublicIds; a filter that matches nothing yields an empty page, not a 404.
        var statusId = await _lookup.GetIdByPublicIdAsync<MatchStatus>(request.StatusPublicId, cancellationToken);
        var stadiumId = await _lookup.GetIdByPublicIdAsync<StadiumEntity>(request.StadiumPublicId, cancellationToken);
        var roundId = await _lookup.GetIdByPublicIdAsync<Round>(request.RoundPublicId, cancellationToken);
        var selectionId = await _lookup.GetIdByPublicIdAsync<Selection>(request.SelectionPublicId, cancellationToken);
        var competitionId = await _lookup.GetIdByPublicIdAsync<CompetitionEntity>(request.CompetitionPublicId, cancellationToken);
        if ((request.StatusPublicId is not null && statusId is null)
            || (request.StadiumPublicId is not null && stadiumId is null)
            || (request.RoundPublicId is not null && roundId is null)
            || (request.SelectionPublicId is not null && selectionId is null)
            || (request.CompetitionPublicId is not null && competitionId is null))
            return new PagedResult<MatchListDto>(Array.Empty<MatchListDto>(), 0, request.Page, request.PageSize);

        var paged = await _match.GetPagedWithFiltersAsync(
            request.Page, request.PageSize,
            new MatchListFilter(statusId, stadiumId, roundId, request.IsPostponed, request.DateFrom, request.DateTo,
                selectionId, competitionId, request.Phase, new SortSpec(request.SortBy, request.SortDir)),
            cancellationToken);

        var items = _mapper.Map<IReadOnlyList<MatchListDto>>(paged.Items);
        return new PagedResult<MatchListDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
