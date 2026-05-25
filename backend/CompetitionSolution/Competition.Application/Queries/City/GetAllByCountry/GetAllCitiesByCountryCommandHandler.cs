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
        public GetAllCitiesByCountryCommandHandler(ICity city, IMapper mapper)
        {
            _mapper = mapper;
            _city = city;
        }

        public async Task<PagedResult<CityListDto>> Handle(GetAllCitiesByCountryCommand request, CancellationToken cancellationToken)
        {
            var paged = await _city.GetPagedByCountryAsync(request.CountryId, request.Page, request.PageSize, cancellationToken);
            var items = _mapper.Map<IReadOnlyList<CityListDto>>(paged.Items);
            return new PagedResult<CityListDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
        }
    }
}
