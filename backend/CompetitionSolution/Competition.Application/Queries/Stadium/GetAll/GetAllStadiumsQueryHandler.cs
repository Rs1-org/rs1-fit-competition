using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.StadiumDto;
using Competition.Application.Interfaces;
using CountryEntity = Competition.Domain.Entities.Country;
using CityEntity = Competition.Domain.Entities.City;
using MediatR;

namespace Competition.Application.Queries.Stadium.GetAll;

public class GetAllStadiumsQueryHandler : IRequestHandler<GetAllStadiumsQuery, PagedResult<StadiumListDto>>
{
    private readonly IStadium _stadium;
    private readonly IMapper _mapper;
    private readonly IPublicIdLookup _lookup;

    public GetAllStadiumsQueryHandler(IStadium stadium, IMapper mapper, IPublicIdLookup lookup)
    {
        _stadium = stadium;
        _mapper = mapper;
        _lookup = lookup;
    }

    public async Task<PagedResult<StadiumListDto>> Handle(GetAllStadiumsQuery request, CancellationToken cancellationToken)
    {
        // Filters arrive as PublicIds; a filter that matches nothing yields an empty page, not a 404.
        var countryId = await _lookup.GetIdByPublicIdAsync<CountryEntity>(request.CountryPublicId, cancellationToken);
        var cityId = await _lookup.GetIdByPublicIdAsync<CityEntity>(request.CityPublicId, cancellationToken);
        if ((request.CountryPublicId is not null && countryId is null)
            || (request.CityPublicId is not null && cityId is null))
            return new PagedResult<StadiumListDto>(Array.Empty<StadiumListDto>(), 0, request.Page, request.PageSize);

        var filter = new StadiumListFilter(request.Name, countryId, cityId, request.MinCapacity, request.MaxCapacity,
            new SortSpec(request.SortBy, request.SortDir));
        var paged = await _stadium.GetPagedFilteredAsync(filter, request.Page, request.PageSize, cancellationToken);
        var items = _mapper.Map<IReadOnlyList<StadiumListDto>>(paged.Items);
        return new PagedResult<StadiumListDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
