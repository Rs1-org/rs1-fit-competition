using AutoMapper;
using Competition.Application.DTOs.CountryDto;
using Competition.Application.Interfaces;
using Competition.Application.Queries.City.GetAll;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Queries.Country.GetBySlug
{
    public class GetCountryBySlugCommandHandler : IRequestHandler<GetCountryBySlugCommand, CountryDetailsDto>
    {
        private readonly IMapper _mapper;
        private readonly ICountry _country;

        public GetCountryBySlugCommandHandler(IMapper mapper, ICountry country)
        {
            _mapper = mapper;
            _country = country;
        }

        public async Task<CountryDetailsDto> Handle(GetCountryBySlugCommand request, CancellationToken cancellationToken)
        {
            var country = await _country.GetBySlugAsync(request.Slug, cancellationToken);

            if (country == null)
                throw new KeyNotFoundException();

            return _mapper.Map<CountryDetailsDto>(country);
        }
    }
}
