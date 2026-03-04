using AutoMapper;
using Competition.Application.DTOs.CountryDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Commands.CountryCommands.Create
{
    public class CreateCountryCommandHandler : IRequestHandler<CreateCountryCommands,CountryNameDto>
    {
        private readonly IMapper _mapper;
        private readonly ICountry _country;
        private readonly IUnitOfWork _unitOfWork;
        public CreateCountryCommandHandler(IMapper mapper, ICountry country, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _country = country;
            _unitOfWork = unitOfWork;
        }

        public async Task<CountryNameDto>Handle(CreateCountryCommands request,CancellationToken cancellationToken)
        {
            var country = _mapper.Map<Country>(request);

            if (country == null)
            {
                throw new ValidationException("Couldn't create country");
            }

            await _country.AddAsync(country);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CountryNameDto>(country);
        }
    }
}
