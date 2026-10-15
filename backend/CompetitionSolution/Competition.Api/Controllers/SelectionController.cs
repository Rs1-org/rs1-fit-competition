using Competition.Application.Commands.SelectionCommands.Create;
using Competition.Application.Commands.SelectionCommands.Delete;
using Competition.Application.Commands.SelectionCommands.Update;
using Competition.Application.Queries.SelectionQueries.GetAll;
using Competition.Application.Queries.SelectionQueries.GetBySlug;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/selection")]
public class SelectionController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] string? countryPublicId = null,
        [FromQuery] string? abbreviation = null,
        [FromQuery] int? followersMin = null,
        [FromQuery] int? followersMax = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAllSelectionsQuery
        {
            Page = page,
            PageSize = pageSize,
            Name = name,
            CountryPublicId = countryPublicId,
            Abbreviation = abbreviation,
            FollowersMin = followersMin,
            FollowersMax = followersMax
        }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSelectionBySlugQuery { Slug = slug }, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSelectionCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{slug}")]
    public async Task<IActionResult> Update(string slug, [FromBody] UpdateSelectionCommand request, CancellationToken cancellationToken)
    {
        request.Slug = slug;
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteSelectionCommand { Id = id }, cancellationToken);
        return Ok(result);
    }
}
