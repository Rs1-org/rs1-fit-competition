using AutoMapper;
using Competition.Application.DTOs.CityDto;
using Competition.Application.Interfaces;
using Competition.Application.Queries.City.GetAllByCountry;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Queries.City.GetBySlug
{
    public class GetCityBySlugCommandHandler : IRequestHandler<GetCityBySlugCommand, CityDetailsDto>
    {
        private readonly ICity _city;
        private readonly IMapper _mapper;
        public GetCityBySlugCommandHandler(ICity city, IMapper mapper)
        {
            _mapper = mapper;
            _city = city;
        }

        public async Task<CityDetailsDto> Handle(GetCityBySlugCommand request, CancellationToken cancellationToken)
        {
            var city = await _city.GetBySlugAsync(request.Slug, cancellationToken);

            if (city == null)
                throw new KeyNotFoundException();

            return _mapper.Map<CityDetailsDto>(city);
        }
    }
}
