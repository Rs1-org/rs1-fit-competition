using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.RoundDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.CompetitionEntities;
using MediatR;

namespace Competition.Application.Queries.Rounds.GetAll;

public class GetAllRoundsQueryHandler : IRequestHandler<GetAllRoundsQuery, PagedResult<RoundListDto>>
{
    private readonly IRound _round;
    private readonly IPublicIdLookup _lookup;
    private readonly IMapper _mapper;

    public GetAllRoundsQueryHandler(IRound round, IPublicIdLookup lookup, IMapper mapper)
    {
        _lookup = lookup;
        _round = round;
        _mapper = mapper;
    }

    public async Task<PagedResult<RoundListDto>> Handle(GetAllRoundsQuery request, CancellationToken cancellationToken)
    {
        // Filter arrives as a PublicId; an unknown one yields an empty page, not a 404.
        var seasonId = await _lookup.GetIdByPublicIdAsync<Season>(request.SeasonPublicId, cancellationToken);
        if (request.SeasonPublicId is not null && seasonId is null)
            return new PagedResult<RoundListDto>(Array.Empty<RoundListDto>(), 0, request.Page, request.PageSize);

        var paged = await _round.GetPagedWithFiltersAsync(
            request.Page, request.PageSize,
            seasonId, request.Search,
            cancellationToken);

        var items = _mapper.Map<IReadOnlyList<RoundListDto>>(paged.Items);
        return new PagedResult<RoundListDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
