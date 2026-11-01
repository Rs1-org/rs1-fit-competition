using Competition.Application.DTOs.PersonDto;
using Competition.Application.Queries.SelectionQueries.GetPlayerAppearances;
using Competition.Application.Commands.PlayerCommands.Create;
using Competition.Application.Commands.PlayerCommands.Delete;
using Competition.Application.Commands.PlayerCommands.Update;
using Competition.Application.Queries.PlayerQueries.GetAll;
using Competition.Application.Queries.PlayerQueries.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[Route("api/player")]
[ApiController]
public class PlayerController(IMediator mediator) : ControllerBase
{
    [HttpGet("{publicId}/appearances")]
    public async Task<IActionResult> Appearances(string publicId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPlayerAppearancesQuery { PublicId = publicId }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{publicId}")]
    public async Task<IActionResult> GetById(string publicId, CancellationToken ct)
    {
        var player = await mediator.Send(new GetPlayerByIdQuery(publicId), ct);
        if (player == null)
            throw new KeyNotFoundException($"Player with public id {publicId} not found");

        return Ok(player);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] string? positionPublicId = null,
        [FromQuery] string? subPositionPublicId = null,
        [FromQuery] int? ageMin = null,
        [FromQuery] int? ageMax = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(new GetAllPlayersQuery
        {
            Page = page,
            PageSize = pageSize,
            Name = name,
            PositionPublicId = positionPublicId,
            SubPositionPublicId = subPositionPublicId,
            AgeMin = ageMin,
            AgeMax = ageMax
        }, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    public async Task<IActionResult> AddPlayer([FromBody] PlayerDto dto, CancellationToken ct)
    {
        var created = await mediator.Send(new CreatePlayerCommand(dto), ct);
        return Ok(created);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{publicId}")]
    public async Task<IActionResult> UpdatePlayer(string publicId, [FromBody] PlayerDto dto, CancellationToken ct)
    {
        var updated = await mediator.Send(new UpdatePlayerCommand(publicId, dto), ct);
        return Ok(updated);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpDelete("{publicId}")]
    public async Task<IActionResult> DeletePlayer(string publicId, CancellationToken ct)
    {
        var deletedId = await mediator.Send(new DeletePlayerCommand(publicId), ct);
        return Ok(deletedId);
    }
}
