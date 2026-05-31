using AutoMapper;
using Competition.Application.DTOs.CityDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Commands.CityCommands.Update
{
    public class UpdateCityCommandHandler : IRequestHandler<UpdateCityCommands,CityNameDto>
    {
        private readonly IMapper _mapper;
        private readonly ICity _city;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCityCommandHandler(IMapper mapper, ICity city, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _city = city;
            _unitOfWork = unitOfWork;
        }

        public async Task<CityNameDto>Handle(UpdateCityCommands request, CancellationToken cancellationToken)
        {
            var city = await _city.GetBySlugAsync(request.Slug, cancellationToken);
            if (city == null)
            {
                throw new KeyNotFoundException($"Couldn't find any city with {request.Slug}");
            }

            city.Name = request.Name;
            city.Slug = SlugHelper.GenerateSlug(request.Name);
            city.Population = request.Population;
            city.IsCapital = request.IsCapital;

            if (city.GeoData != null)
            {
                city.GeoData.Latitude = request.Latitude;
                city.GeoData.Longitude = request.Longitude;
                city.GeoData.Description = request.Description;
            }

            await _city.UpdateAsync(city, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CityNameDto>(city);
        }
    }
}
