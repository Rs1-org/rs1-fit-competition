using AutoMapper;
using Competition.Application.DTOs.CityDto;
using Competition.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Queries.City.GetAll
{
    public class GetAllCitiesCommandHandler : IRequestHandler<GetAllCitiesCommand, List<CityListDto>>
    {
        private readonly ICity _city;
        private readonly IMapper _mapper;
        public GetAllCitiesCommandHandler(ICity city, IMapper mapper)
        {
            _mapper = mapper;
            _city = city;
        }

        public async Task<List<CityListDto>>Handle(GetAllCitiesCommand request, CancellationToken cancellationToken)
        {
            var cities = await _city.GetAllAsync();

            if (cities == null)
                throw new KeyNotFoundException();

            return _mapper.Map<List<CityListDto>>(cities);
        }
    }
}
