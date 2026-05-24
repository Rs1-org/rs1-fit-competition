using AutoMapper;
using Competition.Application.DTOs.CountryDto;
using Competition.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Commands.CountryCommands.Delete
{
    public class DeleteCountryCommandsHandler : IRequestHandler<DeleteCountryCommands, CountryNameDto>
    {
        private readonly ICountry _country;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCountryCommandsHandler(IMapper mapper, ICountry country, IUnitOfWork unitOfWork)
        {
            _country = country;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<CountryNameDto>Handle(DeleteCountryCommands request, CancellationToken cancellationToken)
        {
            var country=await _country.GetByIdAsync(request.Id);

            if (country == null)
                throw new KeyNotFoundException($"Couldn't find any country with {request.Id}");

            await _country.DeleteAsync(country);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CountryNameDto>(country);
        }
    }
}
