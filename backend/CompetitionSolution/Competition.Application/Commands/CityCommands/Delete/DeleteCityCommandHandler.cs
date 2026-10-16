using AutoMapper;
using Competition.Application.DTOs.CityDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
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
        private readonly IPublicIdLookup _lookup;
        private readonly IUnitOfWork _unitOfWork;
        public DeleteCityCommandHandler(ICity city, IMapper mapper, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
        {
            _lookup = lookup;
            _city = city;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<CityNameDto>Handle(DeleteCityCommands request, CancellationToken cancellationToken)
        {
            var city = await _lookup.GetRequiredEntityAsync<City>(request.PublicId, cancellationToken);

            await _city.DeleteAsync(city, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CityNameDto>(city);
            
        }
    }
}
