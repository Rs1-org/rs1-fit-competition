using Competition.Application.DTOs.GalleryDto;
using Microsoft.AspNetCore.Http;

namespace Competition.Application.Interfaces;

public interface IGallery
{
    Task<IReadOnlyList<GallerySectionDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<GalleryImageDto?> GetPersonImageAsync(string kind, Guid id, CancellationToken cancellationToken = default);
    Task<GalleryItemDto> UploadAsync(string kind, Guid id, IFormFile picture, CancellationToken cancellationToken = default);
    Task DeleteAsync(string kind, Guid id, CancellationToken cancellationToken = default);
}
