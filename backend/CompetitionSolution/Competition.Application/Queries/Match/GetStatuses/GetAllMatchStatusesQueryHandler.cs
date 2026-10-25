using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.MatchDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.Match.GetStatuses;

public class GetAllMatchStatusesQueryHandler : IRequestHandler<GetAllMatchStatusesQuery, PagedResult<MatchStatusListDto>>
{
    private readonly IMatchStatus _statuses;
    private readonly IMapper _mapper;

    public GetAllMatchStatusesQueryHandler(IMatchStatus statuses, IMapper mapper)
    {
        _statuses = statuses;
        _mapper = mapper;
    }

    public async Task<PagedResult<MatchStatusListDto>> Handle(GetAllMatchStatusesQuery request, CancellationToken cancellationToken)
    {
        var paged = await _statuses.GetPagedOrderedAsync(request.Page, request.PageSize, cancellationToken);
        var items = _mapper.Map<IReadOnlyList<MatchStatusListDto>>(paged.Items);
        return new PagedResult<MatchStatusListDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
