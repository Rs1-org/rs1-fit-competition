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
    [HttpGet("{id:guid}/appearances")]
    public async Task<IActionResult> Appearances(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPlayerAppearancesQuery { PlayerId = id }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var player = await mediator.Send(new GetPlayerByIdQuery(id), ct);
        if (player == null)
            throw new KeyNotFoundException($"Player with id {id} not found");

        return Ok(player);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] Guid? positionId = null,
        [FromQuery] Guid? subPositionId = null,
        [FromQuery] int? ageMin = null,
        [FromQuery] int? ageMax = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(new GetAllPlayersQuery
        {
            Page = page,
            PageSize = pageSize,
            Name = name,
            PositionId = positionId,
            SubPositionId = subPositionId,
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
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlayer(Guid id, [FromBody] PlayerDto dto, CancellationToken ct)
    {
        var updated = await mediator.Send(new UpdatePlayerCommand(id, dto), ct);
        return Ok(updated);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayer(Guid id, CancellationToken ct)
    {
        var deletedId = await mediator.Send(new DeletePlayerCommand(id), ct);
        return Ok(deletedId);
    }

}
