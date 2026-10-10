using Competition.Application.Commands.FormationMarkCommands.Create;
using Competition.Application.Commands.FormationMarkCommands.Delete;
using Competition.Application.Commands.FormationMarkCommands.Update;
using Competition.Application.Queries.FormationMarkQueries.GetAll;
using Competition.Application.Queries.FormationMarkQueries.GetBySlug;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/formation-mark")]
public class FormationMarkController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAllFormationMarksQuery { Page = page, PageSize = pageSize, Search = search }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetFormationMarkBySlugQuery { Slug = slug }, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFormationMarkCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{slug}")]
    public async Task<IActionResult> Update(string slug, [FromBody] UpdateFormationMarkCommand request, CancellationToken cancellationToken)
    {
        request.Slug = slug;
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteFormationMarkCommand { Id = id }, cancellationToken);
        return Ok(result);
    }
}
