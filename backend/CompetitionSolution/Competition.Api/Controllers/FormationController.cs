using Competition.Application.Commands.FormationCommands.Create;
using Competition.Application.Commands.FormationCommands.Delete;
using Competition.Application.Commands.FormationCommands.Update;
using Competition.Application.Queries.FormationQueries.GetAll;
using Competition.Application.Queries.FormationQueries.GetBySlug;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/formation")]
public class FormationController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAllFormationsQuery { Page = page, PageSize = pageSize, Search = search }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetFormationBySlugQuery { Slug = slug }, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFormationCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{slug}")]
    public async Task<IActionResult> Update(string slug, [FromBody] UpdateFormationCommand request, CancellationToken cancellationToken)
    {
        request.Slug = slug;
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteFormationCommand { Id = id }, cancellationToken);
        return Ok(result);
    }
}
