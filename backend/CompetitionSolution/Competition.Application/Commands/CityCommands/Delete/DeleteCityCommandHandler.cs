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

namespace Competition.Application.Commands.CityCommands.Delete
{
    public class DeleteCityCommandHandler : IRequestHandler<DeleteCityCommands, CityNameDto>
    {
        private readonly ICity _city;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public DeleteCityCommandHandler(ICity city, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _city = city;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<CityNameDto>Handle(DeleteCityCommands request, CancellationToken cancellationToken)
        {
            var city = await _city.GetByIdAsync(request.Id, cancellationToken);

            if (city == null)
            {
                throw new KeyNotFoundException($"Couldn't find any city with {request.Id}");
            }

            await _city.DeleteAsync(city, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CityNameDto>(city);
            
        }
    }
}
