using AutoMapper;
using Competition.Application.DTOs.CountryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<UpdateCountryCommandHandler> _logger;

        public UpdateCountryCommandHandler(IMapper mapper, ICountry country, IUnitOfWork unitOfWork, IImageService imageService, ILogger<UpdateCountryCommandHandler> logger)
        {
            _country = country;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _imageService = imageService;
            _logger = logger;
        }

        public async Task<CountryNameDto> Handle(UpdateCountryCommands request, CancellationToken cancellationToken)
        {
            var country = await _country.GetBySlugAsync(request.Slug, cancellationToken);

            if (country == null)
                throw new KeyNotFoundException($"Couldn't find any country with {request.Slug}");

            country.Name = request.Name;

            country.Slug = SlugHelper.GenerateSlug(request.Name!);

            string? replacedPictureUrl = null;
            string? uploadedPictureUrl = null;
            if (request.Picture != null)
            {
                replacedPictureUrl = country.PictureUrl;
                uploadedPictureUrl = await _imageService.UploadImageAsync(request.Picture, cancellationToken);
                country.PictureUrl = uploadedPictureUrl;
            }

            try
            {
                await _country.UpdateAsync(country, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // The new picture was never saved to the country, so remove it before reporting the failure.
                if (uploadedPictureUrl != null)
                    await TryDeleteImageAsync(uploadedPictureUrl);
                throw;
            }

            // Remove the old picture only once the new one is saved, so a failed update never loses the current image.
            if (!string.IsNullOrEmpty(replacedPictureUrl))
                await TryDeleteImageAsync(replacedPictureUrl);

            return _mapper.Map<CountryNameDto>(country);
        }

        // Blob cleanup must never change the outcome of the update; a failure only leaves an orphaned blob, so log it for manual cleanup.
        // CancellationToken.None on purpose: cleanup should still run when the request was cancelled mid-save.
        private async Task TryDeleteImageAsync(string pictureUrl)
        {
            try
            {
                await _imageService.DeleteImageAsync(pictureUrl, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete country picture {PictureUrl}; the blob is now orphaned", pictureUrl);
            }
        }
    }
}
