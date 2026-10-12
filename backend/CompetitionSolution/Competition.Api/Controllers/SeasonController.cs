using Competition.Application.Commands.SeasonCommands.Create;
using Competition.Application.Commands.SeasonCommands.Delete;
using Competition.Application.Commands.SeasonCommands.Update;
using Competition.Application.Queries.Seasons.GetAll;
using Competition.Application.Queries.Seasons.GetBySlug;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeasonController : ControllerBase
{
    private readonly IMediator _mediator;

    public SeasonController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? competitionId = null,
        [FromQuery] bool? isCurrent = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllSeasonsQuery
        {
            Page = page,
            PageSize = pageSize,
            CompetitionId = competitionId,
            IsCurrent = isCurrent,
            Search = search
        }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("get/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSeasonBySlugQuery { Slug = slug }, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateSeasonCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("update/{slug}")]
    public async Task<IActionResult> Update(string slug, [FromBody] UpdateSeasonCommand request, CancellationToken cancellationToken)
    {
        request.Slug = slug;
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteSeasonCommand { Id = id }, cancellationToken);
        return Ok(result);
    }
}
