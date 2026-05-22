using Competition.Application.Commands.StadiumCommands.Create;
using Competition.Application.Commands.StadiumCommands.Delete;
using Competition.Application.Commands.StadiumCommands.Update;
using Competition.Application.Queries.Stadium.GetAll;
using Competition.Application.Queries.Stadium.GetBySlug;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StadiumController : ControllerBase
{
    private readonly IMediator _mediator;

    public StadiumController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("createStadium")]
    public async Task<IActionResult> CreateStadium([FromForm] CreateStadiumCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("deleteStadium/{id}")]
    public async Task<IActionResult> DeleteStadium(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteStadiumCommand { Id = id }, cancellationToken);
        return Ok(result);
    }

    [HttpPut("updateStadium/{slug}")]
    public async Task<IActionResult> UpdateStadium(string slug, [FromForm] UpdateStadiumCommand request, CancellationToken cancellationToken)
    {
        request.Slug = slug;
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("getStadiums")]
    public async Task<IActionResult> GetStadiums(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllStadiumsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("getStadium/{slug}")]
    public async Task<IActionResult> GetStadium(string slug, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStadiumBySlugQuery { Slug = slug }, cancellationToken);
        return Ok(result);
    }
}
