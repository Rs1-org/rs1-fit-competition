using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.CompetitionDto;
using Competition.Application.Interfaces;
using CountryEntity = Competition.Domain.Entities.Country;
using MediatR;

namespace Competition.Application.Queries.Competitions.GetAll;

public class GetAllCompetitionsQueryHandler : IRequestHandler<GetAllCompetitionsQuery, PagedResult<CompetitionListDto>>
{
    private readonly ICompetition _competition;
    private readonly IPublicIdLookup _lookup;
    private readonly IMapper _mapper;

    public GetAllCompetitionsQueryHandler(ICompetition competition, IPublicIdLookup lookup, IMapper mapper)
    {
        _lookup = lookup;
        _competition = competition;
        _mapper = mapper;
    }

    public async Task<PagedResult<CompetitionListDto>> Handle(GetAllCompetitionsQuery request, CancellationToken cancellationToken)
    {
        // Filter arrives as a PublicId; an unknown one yields an empty page, not a 404.
        var countryId = await _lookup.GetIdByPublicIdAsync<CountryEntity>(request.CountryPublicId, cancellationToken);
        if (request.CountryPublicId is not null && countryId is null)
            return new PagedResult<CompetitionListDto>(Array.Empty<CompetitionListDto>(), 0, request.Page, request.PageSize);

        var paged = await _competition.GetPagedWithFiltersAsync(
            request.Page, request.PageSize,
            request.Search, countryId,
            cancellationToken);

        var items = _mapper.Map<IReadOnlyList<CompetitionListDto>>(paged.Items);
        return new PagedResult<CompetitionListDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
