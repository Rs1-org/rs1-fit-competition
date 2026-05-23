using AutoMapper;
using Competition.Application.DTOs.CountryDto;
using Competition.Application.Interfaces;
using Competition.Application.Services;
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
    public class CreateCountryCommandHandler(
        IMapper mapper,
        ICountry country,
        IUnitOfWork unitOfWork,
        IImageService imageService)
        : IRequestHandler<CreateCountryCommands, CountryNameDto>
    {
        public async Task<CountryNameDto>Handle(CreateCountryCommands request,CancellationToken cancellationToken)
        {
            var pictureUrl = await imageService.UploadImageAsync(request.Picture);
            
            var country1 = mapper.Map<Country>(request);

            if (country1 == null)
            {
                throw new ValidationException("Couldn't create country");
            }


            country1.PictureUrl = pictureUrl;


            await country.AddAsync(country1);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.Map<CountryNameDto>(country1);
        }
    }
}
