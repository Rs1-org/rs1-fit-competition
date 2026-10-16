using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.CityDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.City.GetAll
{
    public class GetAllCitiesCommandHandler : IRequestHandler<GetAllCitiesCommand, PagedResult<CityListDto>>
    {
        private readonly ICity _city;
        private readonly IMapper _mapper;
        private readonly IPublicIdLookup _lookup;
        public GetAllCitiesCommandHandler(ICity city, IMapper mapper, IPublicIdLookup lookup)
        {
            _mapper = mapper;
            _city = city;
            _lookup = lookup;
        }

        public async Task<PagedResult<CityListDto>> Handle(GetAllCitiesCommand request, CancellationToken cancellationToken)
        {
            // The country filter arrives as a PublicId; an unknown one yields an empty page, not a 404.
            var countryId = await _lookup.GetIdByPublicIdAsync<Competition.Domain.Entities.Country>(request.CountryPublicId, cancellationToken);
            if (request.CountryPublicId is not null && countryId is null)
                return new PagedResult<CityListDto>(Array.Empty<CityListDto>(), 0, request.Page, request.PageSize);

            var filter = new CityListFilter(request.Name, countryId, request.IsCapital, request.MinPopulation, request.MaxPopulation);
            var paged = await _city.GetPagedFilteredAsync(filter, request.Page, request.PageSize, cancellationToken);
            var items = _mapper.Map<IReadOnlyList<CityListDto>>(paged.Items);
            return new PagedResult<CityListDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
        }
    }
}
