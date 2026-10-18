using Competition.Application.Commands.ClubCommands;
using Competition.Application.Queries.Clubs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClubController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll([FromQuery] GetAllClubsQuery query, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(query, cancellationToken));

    [HttpGet("get/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetClubBySlugQuery(slug), cancellationToken));

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateClubCommand request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(request, cancellationToken));

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("update/{slug}")]
    public async Task<IActionResult> Update(string slug, UpdateClubCommand request, CancellationToken cancellationToken)
    {
        request.Slug = slug;
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete/{publicId}")]
    public async Task<IActionResult> Delete(string publicId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new DeleteClubCommand(publicId), cancellationToken));
}
