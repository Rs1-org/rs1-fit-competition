using Competition.Application.Commands.PlayerSelectionHistoryCommands;
using Competition.Application.Queries.PlayerSelectionHistoryQueries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[Route("api/player-selection-history")]
[ApiController]
public class PlayerSelectionHistoryController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetPlayerSelectionHistoryByIdQuery(id), cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? playerId = null,
        [FromQuery] Guid? selectionId = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(new GetPlayerSelectionHistoriesQuery
        {
            Page = page,
            PageSize = pageSize,
            PlayerId = playerId,
            SelectionId = selectionId
        }, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreatePlayerSelectionHistoryCommand command, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePlayerSelectionHistoryCommand command, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { Id = id }, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new DeletePlayerSelectionHistoryCommand(id), cancellationToken));
    }
}
