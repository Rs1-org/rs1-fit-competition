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
            var country = await _country.GetBySlugAsync(request.Slug);

            if (country == null)
                throw new KeyNotFoundException($"Couldn't find any country with {request.Slug}");

            country.Name = request.Name;

            if (request.Picture != null)
            {
                await _imageService.DeleteImageAsync(country.PictureUrl);
                var pictureUrl = await _imageService.UploadImageAsync(request.Picture);
                country.PictureUrl = pictureUrl;
            }

            await _country.UpdateAsync(country);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CountryNameDto>(country);
        }
    }
}
