using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.CompetitionDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.Competitions.GetAll;

public class GetAllCompetitionsQueryHandler : IRequestHandler<GetAllCompetitionsQuery, PagedResult<CompetitionListDto>>
{
    private readonly ICompetition _competition;
    private readonly IMapper _mapper;

    public GetAllCompetitionsQueryHandler(ICompetition competition, IMapper mapper)
    {
        _competition = competition;
        _mapper = mapper;
    }

    public async Task<PagedResult<CompetitionListDto>> Handle(GetAllCompetitionsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _competition.GetPagedWithFiltersAsync(
            request.Page, request.PageSize,
            request.Search, request.CountryId,
            cancellationToken);

        var items = _mapper.Map<IReadOnlyList<CompetitionListDto>>(paged.Items);
        return new PagedResult<CompetitionListDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
