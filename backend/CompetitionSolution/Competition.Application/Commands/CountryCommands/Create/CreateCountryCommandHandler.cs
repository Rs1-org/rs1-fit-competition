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
    public class CreateCountryCommandHandler : IRequestHandler<CreateCountryCommands,CountryNameDto>
    {
        private readonly IMapper _mapper;
        private readonly ICountry _country;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;
        public CreateCountryCommandHandler(IMapper mapper, ICountry country, IUnitOfWork unitOfWork, IImageService imageService)
        {
            _mapper = mapper;
            _country = country;
            _unitOfWork = unitOfWork;
            _imageService = imageService;
        }

        public async Task<CountryNameDto>Handle(CreateCountryCommands request,CancellationToken cancellationToken)
        {
            var pictureUrl = await _imageService.UploadImageAsync(request.Picture);
            
            var country = _mapper.Map<Country>(request);

            if (country == null)
            {
                throw new ValidationException("Couldn't create country");
            }


            country.PictureUrl = pictureUrl;


            await _country.AddAsync(country, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CountryNameDto>(country);
        }
    }
}
