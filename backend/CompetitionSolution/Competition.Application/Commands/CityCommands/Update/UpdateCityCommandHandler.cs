using AutoMapper;
using Competition.Application.DTOs.CityDto;
using Competition.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Commands.CityCommands.Update
{
    public class UpdateCityCommandHandler(IMapper mapper, ICity city, IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateCityCommands, CityNameDto>
    {
        public async Task<CityNameDto>Handle(UpdateCityCommands request, CancellationToken cancellationToken)
        {
            var city1 = await city.GetBySlugAsync(request.Slug);
            if(city1== null)
            {
                throw new KeyNotFoundException($"Couldn't find any city with {request.Slug}");
            }
            await city.UpdateAsync(city1);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.Map<CityNameDto>(city1);
        }
    }
}
