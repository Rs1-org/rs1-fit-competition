using Competition.Application.Commands.SubPositionCommands.Create;
using Competition.Application.Commands.SubPositionCommands.Delete;
using Competition.Application.Commands.SubPositionCommands.Update;
using Competition.Application.Queries.SubPositionQueries.GetAll;
using Competition.Application.Queries.SubPositionQueries.GetBySlug;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/subposition")]
public class SubPositionController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? positionId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAllSubPositionsQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            PositionId = positionId
        }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSubPositionBySlugQuery { Slug = slug }, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSubPositionCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{slug}")]
    public async Task<IActionResult> Update(string slug, [FromBody] UpdateSubPositionCommand request, CancellationToken cancellationToken)
    {
        request.Slug = slug;
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteSubPositionCommand { Id = id }, cancellationToken);
        return Ok(result);
    }
}
