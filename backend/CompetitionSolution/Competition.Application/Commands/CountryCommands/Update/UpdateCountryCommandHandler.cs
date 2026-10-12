using AutoMapper;
using Competition.Application.DTOs.CountryDto;
using Competition.Application.Helpers;
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
    public class UpdateCountryCommandHandler : IRequestHandler<UpdateCountryCommands,CountryNameDto>
    {
        private readonly ICountry _country;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;

        public UpdateCountryCommandHandler(IMapper mapper, ICountry country, IUnitOfWork unitOfWork, IImageService imageService)
        {
            _country = country;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _imageService = imageService;
        }

        public async Task<CountryNameDto> Handle(UpdateCountryCommands request, CancellationToken cancellationToken)
        {
            var country = await _country.GetBySlugAsync(request.Slug, cancellationToken);

            if (country == null)
                throw new KeyNotFoundException($"Couldn't find any country with {request.Slug}");

            country.Name = request.Name;

            country.Slug = SlugHelper.GenerateSlug(request.Name!);

            if (request.Picture != null)
            {
                await _imageService.DeleteImageAsync(country.PictureUrl, cancellationToken);
                var pictureUrl = await _imageService.UploadImageAsync(request.Picture, cancellationToken);
                country.PictureUrl = pictureUrl;
            }

            await _country.UpdateAsync(country, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CountryNameDto>(country);
        }
    }
}
