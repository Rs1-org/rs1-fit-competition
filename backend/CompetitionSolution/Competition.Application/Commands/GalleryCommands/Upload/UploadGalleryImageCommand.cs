using Competition.Application.DTOs.GalleryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Competition.Application.Commands.GalleryCommands.Upload;

public class UploadGalleryImageCommand : IRequest<GalleryItemDto>
{
    public string Kind { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public IFormFile? Picture { get; set; }
}

public class UploadGalleryImageCommandHandler : IRequestHandler<UploadGalleryImageCommand, GalleryItemDto>
{
    private readonly IGallery _gallery;

    public UploadGalleryImageCommandHandler(IGallery gallery) => _gallery = gallery;

    public Task<GalleryItemDto> Handle(UploadGalleryImageCommand request, CancellationToken cancellationToken)
    {
        GalleryRules.EnsureKind(request.Kind);
        GalleryRules.EnsurePicture(request.Picture);
        return _gallery.UploadAsync(request.Kind, request.Id, request.Picture!, cancellationToken);
    }
}
