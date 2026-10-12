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
        public GetAllCitiesCommandHandler(ICity city, IMapper mapper)
        {
            _mapper = mapper;
            _city = city;
        }

        public async Task<PagedResult<CityListDto>> Handle(GetAllCitiesCommand request, CancellationToken cancellationToken)
        {
            var filter = new CityListFilter(request.Name, request.CountryId, request.IsCapital, request.MinPopulation, request.MaxPopulation);
            var paged = await _city.GetPagedFilteredAsync(filter, request.Page, request.PageSize, cancellationToken);
            var items = _mapper.Map<IReadOnlyList<CityListDto>>(paged.Items);
            return new PagedResult<CityListDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
        }
    }
}
