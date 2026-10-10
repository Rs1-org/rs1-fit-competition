using Competition.Application.Commands.PositionCommands.Create;
using Competition.Application.Commands.PositionCommands.Delete;
using Competition.Application.Commands.PositionCommands.Update;
using Competition.Application.Queries.PositionQueries.GetAll;
using Competition.Application.Queries.PositionQueries.GetBySlug;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/position")]
public class PositionController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAllPositionsQuery { Page = page, PageSize = pageSize, Search = search }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPositionBySlugQuery { Slug = slug }, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePositionCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{slug}")]
    public async Task<IActionResult> Update(string slug, [FromBody] UpdatePositionCommand request, CancellationToken cancellationToken)
    {
        request.Slug = slug;
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeletePositionCommand { Id = id }, cancellationToken);
        return Ok(result);
    }
}
