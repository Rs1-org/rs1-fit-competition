using Competition.Application.Commands.GalleryCommands.Delete;
using Competition.Application.Commands.GalleryCommands.Upload;
using Competition.Application.Queries.GalleryQueries.GetGallery;
using Competition.Application.Queries.GalleryQueries.GetGalleryImage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Moderator")]
[Route("api/gallery")]
public class GalleryController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetGalleryQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{kind}/{publicId}/image")]
    public async Task<IActionResult> Image(string kind, string publicId, CancellationToken cancellationToken)
    {
        var image = await mediator.Send(new GetGalleryImageQuery { Kind = kind, PublicId = publicId }, cancellationToken);
        if (image == null)
            throw new KeyNotFoundException("Image was not found.");

        return File(image.Bytes, image.ContentType);
    }

    [HttpPost("{kind}/{publicId}")]
    [RequestSizeLimit(8 * 1024 * 1024)]
    public async Task<IActionResult> Upload(string kind, string publicId, IFormFile picture, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UploadGalleryImageCommand
        {
            Kind = kind,
            PublicId = publicId,
            Picture = picture
        }, cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{kind}/{publicId}")]
    public async Task<IActionResult> Delete(string kind, string publicId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteGalleryImageCommand { Kind = kind, PublicId = publicId }, cancellationToken);
        return NoContent();
    }
}
