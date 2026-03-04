using AutoMapper;
using Competition.Application.DTOs.CityDto;
using Competition.Application.Interfaces;
using Competition.Application.Queries.City.GetAll;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Queries.City.GetAllByCountry
{
    public class GetAllCitiesByCountryCommandHandler : IRequestHandler<GetAllCitiesByCountryCommand, List<CityListDto>>
    {
        private readonly ICity _city;
        private readonly IMapper _mapper;
        public GetAllCitiesByCountryCommandHandler(ICity city, IMapper mapper)
        {
            _mapper = mapper;
            _city = city;
        }

        public async Task<List<CityListDto>> Handle(GetAllCitiesByCountryCommand request, CancellationToken cancellationToken)
        {
            var cities = await _city.GetAllByCountryAsync(request.CountryId);

            if (cities == null)
                throw new KeyNotFoundException();

            return _mapper.Map<List<CityListDto>>(cities);
        }
    }
}
