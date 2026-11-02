using Competition.Application.Commands.MatchCommands.Create;
using Competition.Application.Commands.MatchCommands.Delete;
using Competition.Application.Commands.MatchCommands.Import;
using Competition.Application.Commands.MatchCommands.Update;
using Competition.Application.Commands.MatchLineupCommands;
using Competition.Application.Queries.Match.Export;
using Competition.Application.Queries.Match.GetAll;
using Competition.Application.Queries.Match.GetById;
using Competition.Application.Queries.Match.GetBySlug;
using Competition.Application.Queries.Reports;
using Competition.Application.Lineups;
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
        [FromQuery] string? selectionPublicId = null,
        [FromQuery] string? competitionPublicId = null,
        [FromQuery] string? phase = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
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
            DateTo = dateTo,
            SelectionPublicId = selectionPublicId,
            CompetitionPublicId = competitionPublicId,
            Phase = phase,
            SortBy = sortBy,
            SortDir = sortDir
        }, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] ExportMatchesQuery query, CancellationToken cancellationToken)
    {
        var file = await _mediator.Send(query, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("import")]
    [RequestSizeLimit(2 * 1024 * 1024 + 64 * 1024)]
    public async Task<IActionResult> Import([FromForm] ImportMatchesCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("report")]
    public async Task<IActionResult> MatchesReport([FromQuery] GetMatchesReportQuery query, CancellationToken cancellationToken)
    {
        var file = await _mediator.Send(query, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("report/{publicId}")]
    public async Task<IActionResult> MatchReport(string publicId, CancellationToken cancellationToken)
    {
        var file = await _mediator.Send(new GetMatchReportQuery(publicId), cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
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

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateMatchCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("update/{slug}")]
    public async Task<IActionResult> Update(string slug, [FromBody] UpdateMatchCommand request, CancellationToken cancellationToken)
    {
        request.Slug = slug;
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{publicId}/lineup")]
    public async Task<IActionResult> GetLineup(string publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMatchLineupQuery(publicId), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{publicId}/lineup")]
    public async Task<IActionResult> SaveLineup(string publicId, [FromBody] SaveMatchLineupCommand request, CancellationToken cancellationToken)
    {
        request.PublicId = publicId;
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{publicId}/referee")]
    public async Task<IActionResult> AssignReferee(string publicId, [FromBody] AssignMatchRefereeCommand request, CancellationToken cancellationToken)
    {
        request.PublicId = publicId;
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
