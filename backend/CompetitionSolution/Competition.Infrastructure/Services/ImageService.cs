using Competition.Application.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition.Infrastructure.Services
{
    public class ImageService : IImageService
    {
        private readonly IBlobStorageService _blobStorageService;

        public ImageService(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }
        public async Task DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default)
        {
            await _blobStorageService.DeleteAsync(imageUrl, cancellationToken);
        }

        public async Task<string> UploadImageAsync(IFormFile image, CancellationToken cancellationToken = default)
        {
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(image.ContentType))
                throw new Exception("Invalid file type");

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";

            return await _blobStorageService.UploadAsync(image.OpenReadStream(), fileName, image.ContentType, cancellationToken);
        }
    }
}
