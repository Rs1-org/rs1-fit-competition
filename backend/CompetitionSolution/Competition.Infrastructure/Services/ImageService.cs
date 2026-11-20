using Competition.Application.Helpers;
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
            if (image.Length > GalleryRules.MaxBytes)
                throw BusinessRule.Violation("picture", "File is too large.");

            await using var stream = image.OpenReadStream();
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory, cancellationToken);
            var bytes = memory.ToArray();
            GalleryRules.EnsurePictureBytes(bytes, image.ContentType);

            var contentType = GalleryRules.ContentTypeOf(bytes);
            var extension = contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".bin"
            };
            var fileName = $"{Guid.NewGuid()}{extension}";
            memory.Position = 0;

            return await _blobStorageService.UploadAsync(memory, fileName, contentType, cancellationToken);
        }
    }
}
