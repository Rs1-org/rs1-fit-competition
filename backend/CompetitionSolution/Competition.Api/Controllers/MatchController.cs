using Competition.Application.Commands.MatchCommands.Create;
using Competition.Application.Commands.MatchCommands.Delete;
using Competition.Application.Commands.MatchCommands.Update;
using Competition.Application.Queries.Match.GetAll;
using Competition.Application.Queries.Match.GetById;
using Competition.Application.Queries.Match.GetBySlug;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchController : ControllerBase
{
    private readonly IMediator _mediator;

    public MatchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? statusPublicId = null,
        [FromQuery] string? stadiumPublicId = null,
        [FromQuery] string? roundPublicId = null,
        [FromQuery] bool? isPostponed = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllMatchesQuery
        {
            Page = page,
            PageSize = pageSize,
            StatusPublicId = statusPublicId,
            StadiumPublicId = stadiumPublicId,
            RoundPublicId = roundPublicId,
            IsPostponed = isPostponed,
            DateFrom = dateFrom,
            DateTo = dateTo
        }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("get/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMatchBySlugQuery { Slug = slug }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("getByPublicId/{publicId}")]
    public async Task<IActionResult> GetByPublicId(string publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMatchByPublicIdQuery { PublicId = publicId }, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateMatchCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("update/{slug}")]
    public async Task<IActionResult> Update(string slug, [FromBody] UpdateMatchCommand request, CancellationToken cancellationToken)
    {
        request.Slug = slug;
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete/{publicId}")]
    public async Task<IActionResult> Delete(string publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteMatchCommand { PublicId = publicId }, cancellationToken);
        return Ok(result);
    }
}
