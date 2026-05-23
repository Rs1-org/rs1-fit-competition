using Competition.Application.Commands.ClubCommands.Create;
using Competition.Application.Commands.ClubCommands.Delete;
using Competition.Application.Commands.ClubCommands.Update;
using Competition.Application.Queries.Club.GetAll;
using Competition.Application.Queries.Club.GetBySlug;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClubController(IMediator mediator) : ControllerBase
{
    [HttpPost("create")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateClub([FromBody] CreateClubCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("update/{slug}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateClub(string slug, [FromBody] UpdateClubCommand request, CancellationToken cancellationToken)
    {
        request.Slug = slug;
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("delete/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteClub(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteClubCommand { Id = id }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("getAll")]
    [AllowAnonymous]
    public async Task<IActionResult> GetClubs(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllClubsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("get/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetClub(string slug, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetClubBySlugQuery { Slug = slug }, cancellationToken);
        return Ok(result);
    }
}
