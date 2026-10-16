using Competition.Application.Commands.RoundCommands.Create;
using Competition.Application.Commands.RoundCommands.Delete;
using Competition.Application.Commands.RoundCommands.Update;
using Competition.Application.Queries.Rounds.GetAll;
using Competition.Application.Queries.Rounds.GetBySlug;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoundController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoundController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? seasonPublicId = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllRoundsQuery
        {
            Page = page,
            PageSize = pageSize,
            SeasonPublicId = seasonPublicId,
            Search = search
        }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("get/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRoundBySlugQuery { Slug = slug }, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateRoundCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("update/{slug}")]
    public async Task<IActionResult> Update(string slug, [FromBody] UpdateRoundCommand request, CancellationToken cancellationToken)
    {
        request.Slug = slug;
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete/{publicId}")]
    public async Task<IActionResult> Delete(string publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteRoundCommand { PublicId = publicId }, cancellationToken);
        return Ok(result);
    }
}
