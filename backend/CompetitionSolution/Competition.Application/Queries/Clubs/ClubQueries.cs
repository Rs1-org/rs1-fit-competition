using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.ClubDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using CountryEntity = Competition.Domain.Entities.Country;
using CityEntity = Competition.Domain.Entities.City;
using MediatR;

namespace Competition.Application.Queries.Clubs;

public class GetAllClubsQuery : IRequest<PagedResult<ClubListDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Name { get; init; }
    public string? CountryPublicId { get; init; }
    public string? CityPublicId { get; init; }
    public string? FoundedFrom { get; init; }
    public string? FoundedTo { get; init; }
    public bool? HasStadium { get; init; }
    public ClubAge? ClubAge { get; init; }
    public string SortBy { get; init; } = "name";
    public bool Descending { get; init; }
}

public record GetClubBySlugQuery(string Slug) : IRequest<ClubDetailsDto>;

public class GetAllClubsQueryHandler(IClub clubs, IPublicIdLookup lookup, IMapper mapper)
    : IRequestHandler<GetAllClubsQuery, PagedResult<ClubListDto>>
{
    public async Task<PagedResult<ClubListDto>> Handle(GetAllClubsQuery request, CancellationToken cancellationToken)
    {
        var countryId = await lookup.GetIdByPublicIdAsync<CountryEntity>(request.CountryPublicId, cancellationToken);
        var cityId = await lookup.GetIdByPublicIdAsync<CityEntity>(request.CityPublicId, cancellationToken);
        if ((request.CountryPublicId is not null && countryId is null)
            || (request.CityPublicId is not null && cityId is null))
            return new PagedResult<ClubListDto>([], 0, request.Page, request.PageSize);

        var filter = new ClubListFilter(request.Name, countryId, cityId,
            NodaTimeMappingHelper.ParseNullableLocalDate(request.FoundedFrom),
            NodaTimeMappingHelper.ParseNullableLocalDate(request.FoundedTo),
            request.HasStadium, request.ClubAge, request.SortBy, request.Descending);
        var page = await clubs.GetPagedFilteredAsync(filter, request.Page, request.PageSize, cancellationToken);
        return new PagedResult<ClubListDto>(mapper.Map<IReadOnlyList<ClubListDto>>(page.Items),
            page.TotalCount, page.Page, page.PageSize);
    }
}

public class GetClubBySlugQueryHandler(IClub clubs, IMapper mapper) : IRequestHandler<GetClubBySlugQuery, ClubDetailsDto>
{
    public async Task<ClubDetailsDto> Handle(GetClubBySlugQuery request, CancellationToken cancellationToken) =>
        mapper.Map<ClubDetailsDto>(await clubs.GetBySlugAsync(request.Slug, cancellationToken)
            ?? throw new KeyNotFoundException("Club not found."));
}
