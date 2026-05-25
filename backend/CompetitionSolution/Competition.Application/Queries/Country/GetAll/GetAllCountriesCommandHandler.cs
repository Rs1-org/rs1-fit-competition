using AutoMapper;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.CountryDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.Country.GetAll
{
    public class GetAllCountriesCommandHandler : IRequestHandler<GetAllCountriesCommand, PagedResult<CountryListDto>>
    {
        private readonly IMapper _mapper;
        private readonly ICountry _country;

        public GetAllCountriesCommandHandler(IMapper mapper, ICountry country)
        {
            _mapper = mapper;
            _country = country;
        }

        public async Task<PagedResult<CountryListDto>> Handle(GetAllCountriesCommand request, CancellationToken cancellationToken)
        {
            var paged = await _country.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
            var items = _mapper.Map<IReadOnlyList<CountryListDto>>(paged.Items);
            return new PagedResult<CountryListDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
        }
    }
}
