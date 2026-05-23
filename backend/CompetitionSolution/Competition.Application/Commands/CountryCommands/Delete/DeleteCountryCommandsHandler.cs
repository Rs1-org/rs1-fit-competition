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
    internal class DeleteCountryCommandsHandler(IMapper mapper, ICountry country, IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteCountryCommands, CountryNameDto>
    {
        public async Task<CountryNameDto>Handle(DeleteCountryCommands request, CancellationToken cancellationToken)
        {
            var country1=await country.GetByIdAsync(request.Id);

            if (country1 == null)
                throw new KeyNotFoundException($"Couldn't find any city with {request.Id}");

            await country.DeleteAsync(country1);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.Map<CountryNameDto>(country1);
        }
    }
}
