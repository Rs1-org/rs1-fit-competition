using Competition.Application.Commands.StadiumCommands.Create;
using Competition.Application.Commands.StadiumCommands.Delete;
using Competition.Application.Commands.StadiumCommands.Update;
using Competition.Application.Queries.Stadium.GetAll;
using Competition.Application.Queries.Stadium.GetBySlug;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost("createStadium")]
    public async Task<IActionResult> CreateStadium([FromForm] CreateStadiumCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("deleteStadium/{publicId}")]
    public async Task<IActionResult> DeleteStadium(string publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteStadiumCommand { PublicId = publicId }, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("updateStadium/{slug}")]
    public async Task<IActionResult> UpdateStadium(string slug, [FromForm] UpdateStadiumCommand request, CancellationToken cancellationToken)
    {
        request.Slug = slug;
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("getStadiums")]
    public async Task<IActionResult> GetStadiums(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] string? countryPublicId = null,
        [FromQuery] string? cityPublicId = null,
        [FromQuery] int? minCapacity = null,
        [FromQuery] int? maxCapacity = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllStadiumsQuery
        {
            Page = page,
            PageSize = pageSize,
            Name = name,
            CountryPublicId = countryPublicId,
            CityPublicId = cityPublicId,
            MinCapacity = minCapacity,
            MaxCapacity = maxCapacity
        }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("getStadium/{slug}")]
    public async Task<IActionResult> GetStadium(string slug, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStadiumBySlugQuery { Slug = slug }, cancellationToken);
        return Ok(result);
    }
}
