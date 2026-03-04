using AutoMapper;
using Competition.Application.DTOs.CountryDto;
using Competition.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Queries.City.GetAll
{
    public class GetAllCountriesCommandHandler : IRequestHandler<GetAllCountriesCommand, List<CountryListDto>>
    {
        private readonly IMapper _mapper;
        private readonly ICountry _country;

        public GetAllCountriesCommandHandler(IMapper mapper, ICountry country)
        {
            _mapper = mapper;
            _country = country;
        }

        public async Task<List<CountryListDto>> Handle(GetAllCountriesCommand request, CancellationToken cancellationToken)
        {
            var countries = await _country.GetAllAsync();

            if (countries == null)
                throw new KeyNotFoundException();

            return _mapper.Map<List<CountryListDto>>(countries);
        }
    }
}
