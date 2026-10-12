using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.MatchDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.Match.GetAll;

public class GetAllMatchesQueryHandler : IRequestHandler<GetAllMatchesQuery, PagedResult<MatchListDto>>
{
    private readonly IMatch _match;
    private readonly IMapper _mapper;

    public GetAllMatchesQueryHandler(IMatch match, IMapper mapper)
    {
        _match = match;
        _mapper = mapper;
    }

    public async Task<PagedResult<MatchListDto>> Handle(GetAllMatchesQuery request, CancellationToken cancellationToken)
    {
        var paged = await _match.GetPagedWithFiltersAsync(
            request.Page, request.PageSize,
            request.StatusId, request.StadiumId, request.RoundId,
            request.IsPostponed,
            request.DateFrom, request.DateTo,
            cancellationToken);

        var items = _mapper.Map<IReadOnlyList<MatchListDto>>(paged.Items);
        return new PagedResult<MatchListDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
