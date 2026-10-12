using Competition.Application.Commands.ClubCommands.Create;
using Competition.Application.Commands.ClubCommands.Update;
using Competition.Application.Commands.ClubCommands.Delete;
using Competition.Application.Queries.Club.GetAll;
using Competition.Application.Queries.Club.GetBySlug;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/club")]
public class ClubController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllClubsQuery query, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(query, cancellationToken));

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetClubBySlugQuery { Slug = slug }, cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> Create(CreateClubCommand command, CancellationToken cancellationToken)
    {
        var club = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetBySlug), new { slug = club.Slug }, club);
    }

    [HttpPut("{slug}")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> Update(string slug, UpdateClubCommand command, CancellationToken cancellationToken)
    {
        command.Slug = slug;
        return Ok(await mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new DeleteClubCommand { Id = id }, cancellationToken));
}
