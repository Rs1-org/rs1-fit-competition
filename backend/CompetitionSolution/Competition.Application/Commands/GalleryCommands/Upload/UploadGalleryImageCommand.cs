using Competition.Application.Common.Validation;
using Competition.Application.DTOs.GalleryDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Competition.Application.Commands.GalleryCommands.Upload;

public class UploadGalleryImageCommand : IRequest<GalleryItemDto>
{
    public string Kind { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
    public IFormFile? Picture { get; set; }
}

public class UploadGalleryImageCommandHandler : IRequestHandler<UploadGalleryImageCommand, GalleryItemDto>
{
    private readonly IGallery _gallery;
    private readonly IPublicIdLookup _lookup;

    public UploadGalleryImageCommandHandler(IGallery gallery, IPublicIdLookup lookup)
    {
        _gallery = gallery;
        _lookup = lookup;
    }

    public async Task<GalleryItemDto> Handle(UploadGalleryImageCommand request, CancellationToken cancellationToken)
    {
        GalleryRules.EnsureKind(request.Kind);
        GalleryRules.EnsurePicture(request.Picture);
        var id = await GalleryIds.ResolveAsync(_lookup, request.Kind, request.PublicId, cancellationToken);
        return await _gallery.UploadAsync(request.Kind, id, request.Picture!, cancellationToken);
    }
}

public class UploadGalleryImageCommandValidator : AbstractValidator<UploadGalleryImageCommand>
{
    public UploadGalleryImageCommandValidator()
    {
        RuleFor(x => x.Kind)
            .Must(GalleryRules.IsKnown)
            .WithMessage("Unknown image type.");

        RuleFor(x => x.PublicId).MustBeValidPublicId();

        RuleFor(x => x.Picture).Custom((file, context) =>
        {
            try
            {
                GalleryRules.EnsurePicture(file);
            }
            catch (ValidationException exception)
            {
                foreach (var failure in exception.Errors)
                    context.AddFailure(failure);
            }
        });
    }
}
