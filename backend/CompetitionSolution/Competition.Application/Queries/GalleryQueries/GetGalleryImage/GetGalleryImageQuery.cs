using Competition.Application.DTOs.GalleryDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.GalleryQueries.GetGalleryImage;

public class GetGalleryImageQuery : IRequest<GalleryImageDto?>
{
    public string Kind { get; set; } = string.Empty;
    public Guid Id { get; set; }
}

public class GetGalleryImageQueryHandler : IRequestHandler<GetGalleryImageQuery, GalleryImageDto?>
{
    private readonly IGallery _gallery;

    public GetGalleryImageQueryHandler(IGallery gallery) => _gallery = gallery;

    public Task<GalleryImageDto?> Handle(GetGalleryImageQuery request, CancellationToken cancellationToken) =>
        _gallery.GetPersonImageAsync(request.Kind, request.Id, cancellationToken);
}
