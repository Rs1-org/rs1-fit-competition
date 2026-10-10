using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;

namespace Competition.Application.Commands.GalleryCommands.Delete;

public class DeleteGalleryImageCommand : IRequest<Unit>
{
    public string Kind { get; set; } = string.Empty;
    public Guid Id { get; set; }
}

public class DeleteGalleryImageCommandHandler : IRequestHandler<DeleteGalleryImageCommand, Unit>
{
    private readonly IGallery _gallery;

    public DeleteGalleryImageCommandHandler(IGallery gallery) => _gallery = gallery;

    public async Task<Unit> Handle(DeleteGalleryImageCommand request, CancellationToken cancellationToken)
    {
        GalleryRules.EnsureKind(request.Kind);
        await _gallery.DeleteAsync(request.Kind, request.Id, cancellationToken);
        return Unit.Value;
    }
}
