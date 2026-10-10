using Competition.Application.DTOs.GalleryDto;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Queries.GalleryQueries.GetGallery;

public class GetGalleryQuery : IRequest<IReadOnlyList<GallerySectionDto>>
{
}

public class GetGalleryQueryHandler : IRequestHandler<GetGalleryQuery, IReadOnlyList<GallerySectionDto>>
{
    private readonly IGallery _gallery;

    public GetGalleryQueryHandler(IGallery gallery) => _gallery = gallery;

    public Task<IReadOnlyList<GallerySectionDto>> Handle(GetGalleryQuery request, CancellationToken cancellationToken) =>
        _gallery.ListAsync(cancellationToken);
}
