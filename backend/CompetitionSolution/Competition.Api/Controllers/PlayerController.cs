using Competition.Application.DTOs.PersonDto;
using Competition.Application.Features.PlayerCommands.Create;
using Competition.Application.Features.PlayerCommands.Delete;
using Competition.Application.Features.PlayerCommands.Update;
using Competition.Application.Features.PlayerQueries.GetAll;
using Competition.Application.Features.PlayerQueries.GetById;
using Competition.Application.Features.PlayerQueries.Search;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[Route("api/player")]
[ApiController]
public class PlayerController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var player = await mediator.Send(new GetPlayerByIdQuery(id));
        if (player == null)
            throw new KeyNotFoundException($"Player with id {id} not found");

        return Ok(player);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await mediator.Send(new GetAllPlayersQuery()));
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddPlayer([FromBody] PlayerDto dto)
    {
        var created = await mediator.Send(new CreatePlayerCommand(dto));
        return Ok(created);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlayer(Guid id, [FromBody] PlayerDto dto)
    {
        var updated = await mediator.Send(new UpdatePlayerCommand(id, dto));
        return Ok(updated);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayer(Guid id)
    {
        var deletedId = await mediator.Send(new DeletePlayerCommand(id));
        return Ok(deletedId);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return Ok(await mediator.Send(new GetAllPlayersQuery()));

        return Ok(await mediator.Send(new SearchPlayersQuery(term)));
    }
}
