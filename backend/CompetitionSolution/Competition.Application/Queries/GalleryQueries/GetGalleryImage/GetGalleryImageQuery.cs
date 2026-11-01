using Competition.Application.Common.Validation;
using Competition.Application.DTOs.GalleryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Competition.Application.Queries.GalleryQueries.GetGalleryImage;

public class GetGalleryImageQuery : IRequest<GalleryImageDto?>
{
    public string Kind { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
}

public class GetGalleryImageQueryValidator : AbstractValidator<GetGalleryImageQuery>
{
    public GetGalleryImageQueryValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}

public class GetGalleryImageQueryHandler : IRequestHandler<GetGalleryImageQuery, GalleryImageDto?>
{
    private readonly IGallery _gallery;
    private readonly IPublicIdLookup _lookup;

    public GetGalleryImageQueryHandler(IGallery gallery, IPublicIdLookup lookup)
    {
        _gallery = gallery;
        _lookup = lookup;
    }

    public async Task<GalleryImageDto?> Handle(GetGalleryImageQuery request, CancellationToken cancellationToken)
    {
        GalleryRules.EnsureKind(request.Kind);
        var id = await GalleryIds.ResolveAsync(_lookup, request.Kind, request.PublicId, cancellationToken);
        return await _gallery.GetPersonImageAsync(request.Kind, id, cancellationToken);
    }
}
