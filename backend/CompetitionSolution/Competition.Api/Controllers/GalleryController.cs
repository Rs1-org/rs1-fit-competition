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

    [HttpGet("{kind}/{id:guid}/image")]
    public async Task<IActionResult> Image(string kind, Guid id, CancellationToken cancellationToken)
    {
        var image = await mediator.Send(new GetGalleryImageQuery { Kind = kind, Id = id }, cancellationToken);
        if (image == null)
            throw new KeyNotFoundException("Image was not found.");

        return File(image.Bytes, image.ContentType);
    }

    [HttpPost("{kind}/{id:guid}")]
    [RequestSizeLimit(8 * 1024 * 1024)]
    public async Task<IActionResult> Upload(string kind, Guid id, IFormFile picture, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UploadGalleryImageCommand
        {
            Kind = kind,
            Id = id,
            Picture = picture
        }, cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{kind}/{id:guid}")]
    public async Task<IActionResult> Delete(string kind, Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteGalleryImageCommand { Kind = kind, Id = id }, cancellationToken);
        return NoContent();
    }
}
