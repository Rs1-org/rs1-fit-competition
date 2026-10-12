using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.RoundDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.Rounds.GetAll;

public class GetAllRoundsQueryHandler : IRequestHandler<GetAllRoundsQuery, PagedResult<RoundListDto>>
{
    private readonly IRound _round;
    private readonly IMapper _mapper;

    public GetAllRoundsQueryHandler(IRound round, IMapper mapper)
    {
        _round = round;
        _mapper = mapper;
    }

    public async Task<PagedResult<RoundListDto>> Handle(GetAllRoundsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _round.GetPagedWithFiltersAsync(
            request.Page, request.PageSize,
            request.SeasonId, request.Search,
            cancellationToken);

        var items = _mapper.Map<IReadOnlyList<RoundListDto>>(paged.Items);
        return new PagedResult<RoundListDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
