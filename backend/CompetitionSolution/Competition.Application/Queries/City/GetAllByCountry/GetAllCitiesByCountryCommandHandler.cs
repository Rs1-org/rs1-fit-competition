using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.CityDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.City.GetAllByCountry
{
    public class GetAllCitiesByCountryCommandHandler : IRequestHandler<GetAllCitiesByCountryCommand, PagedResult<CityListDto>>
    {
        private readonly ICity _city;
        private readonly IMapper _mapper;
        private readonly IPublicIdLookup _lookup;
        public GetAllCitiesByCountryCommandHandler(ICity city, IMapper mapper, IPublicIdLookup lookup)
        {
            _lookup = lookup;
            _mapper = mapper;
            _city = city;
        }

        public async Task<PagedResult<CityListDto>> Handle(GetAllCitiesByCountryCommand request, CancellationToken cancellationToken)
        {
            var countryId = await _lookup.GetRequiredIdAsync<Domain.Entities.Country>(request.CountryPublicId, cancellationToken);
            var paged = await _city.GetPagedByCountryAsync(countryId, request.Page, request.PageSize, cancellationToken);
            var items = _mapper.Map<IReadOnlyList<CityListDto>>(paged.Items);
            return new PagedResult<CityListDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
        }
    }
}
