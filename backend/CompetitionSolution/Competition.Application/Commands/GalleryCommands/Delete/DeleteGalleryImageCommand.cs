using Competition.Application.Common.Validation;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Competition.Application.Commands.GalleryCommands.Delete;

public class DeleteGalleryImageCommand : IRequest<Unit>
{
    public string Kind { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
}

public class DeleteGalleryImageCommandHandler : IRequestHandler<DeleteGalleryImageCommand, Unit>
{
    private readonly IGallery _gallery;
    private readonly IPublicIdLookup _lookup;

    public DeleteGalleryImageCommandHandler(IGallery gallery, IPublicIdLookup lookup)
    {
        _gallery = gallery;
        _lookup = lookup;
    }

    public async Task<Unit> Handle(DeleteGalleryImageCommand request, CancellationToken cancellationToken)
    {
        GalleryRules.EnsureKind(request.Kind);
        var id = await GalleryIds.ResolveAsync(_lookup, request.Kind, request.PublicId, cancellationToken);
        await _gallery.DeleteAsync(request.Kind, id, cancellationToken);
        return Unit.Value;
    }
}

public class DeleteGalleryImageCommandValidator : AbstractValidator<DeleteGalleryImageCommand>
{
    public DeleteGalleryImageCommandValidator()
    {
        RuleFor(x => x.PublicId).MustBeValidPublicId();
        RuleFor(x => x.Kind)
            .Must(GalleryRules.IsKnown)
            .WithMessage("Unknown image type.");
    }
}
