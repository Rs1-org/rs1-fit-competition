using AutoMapper;
using Competition.Application.DTOs.CountryDto;
using Competition.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Commands.CountryCommands.Update
{
    public class UpdateCountryCommandHandler : IRequestHandler<UpdateCountryCommands,CountryNameDto>
    {
        private readonly ICountry _country;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCountryCommandHandler(IMapper mapper, ICountry country, IUnitOfWork unitOfWork)
        {
            _country = country;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<CountryNameDto> Handle(UpdateCountryCommands request, CancellationToken cancellationToken)
        {
            var country = await _country.GetBySlugAsync(request.Slug);

            if (country == null)
                throw new KeyNotFoundException($"Couldn't find any city with {request.Slug}");

            await _country.UpdateAsync(country);
            _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CountryNameDto>(country);
        }
    }
}
