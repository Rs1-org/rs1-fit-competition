using AutoMapper;
using Competition.Application.DTOs.CountryDto;
using Competition.Application.Interfaces;
using Competition.Application.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Application.Commands.CountryCommands.Update
{
    public class UpdateCountryCommandHandler(
        IMapper mapper,
        ICountry country,
        IUnitOfWork unitOfWork,
        IImageService imageService)
        : IRequestHandler<UpdateCountryCommands, CountryNameDto>
    {
        public async Task<CountryNameDto> Handle(UpdateCountryCommands request, CancellationToken cancellationToken)
        {
            var country1 = await country.GetBySlugAsync(request.Slug);

            if (country1 == null)
                throw new KeyNotFoundException($"Couldn't find any city with {request.Slug}");

            if (request.Picture != null)
            {
                await imageService.DeleteImageAsync(country1.PictureUrl);
                var pictureUrl = await imageService.UploadImageAsync(request.Picture);
                country1.PictureUrl = pictureUrl;
            }

            await country.UpdateAsync(country1);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.Map<CountryNameDto>(country1);
        }
    }
}
