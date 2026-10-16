using AutoMapper;
using Competition.Application.DTOs.CountryDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
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
        private readonly IPublicIdLookup _lookup;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCountryCommandsHandler(IMapper mapper, ICountry country, IUnitOfWork unitOfWork, IPublicIdLookup lookup)
        {
            _lookup = lookup;
            _country = country;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<CountryNameDto>Handle(DeleteCountryCommands request, CancellationToken cancellationToken)
        {
            var country = await _lookup.GetRequiredEntityAsync<Country>(request.PublicId, cancellationToken);

            await _country.DeleteAsync(country, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CountryNameDto>(country);
        }
    }
}
