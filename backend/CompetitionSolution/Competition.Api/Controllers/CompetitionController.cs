using Competition.Application.Commands.CompetitionCommands.Create;
using Competition.Application.Commands.CompetitionCommands.Delete;
using Competition.Application.Commands.CompetitionCommands.Update;
using Competition.Application.Queries.Competitions.GetAll;
using Competition.Application.Queries.Competitions.GetBySlug;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompetitionController : ControllerBase
{
    private readonly IMediator _mediator;

    public CompetitionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? countryPublicId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllCompetitionsQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            CountryPublicId = countryPublicId
        }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("get/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCompetitionBySlugQuery { Slug = slug }, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateCompetitionCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("update/{slug}")]
    public async Task<IActionResult> Update(string slug, [FromBody] UpdateCompetitionCommand request, CancellationToken cancellationToken)
    {
        request.Slug = slug;
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete/{publicId}")]
    public async Task<IActionResult> Delete(string publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteCompetitionCommand { PublicId = publicId }, cancellationToken);
        return Ok(result);
    }
}
