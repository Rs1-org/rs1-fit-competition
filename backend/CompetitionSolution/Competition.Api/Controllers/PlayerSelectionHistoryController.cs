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
    [HttpGet("{publicId}")]
    public async Task<IActionResult> GetById(string publicId, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetPlayerSelectionHistoryByIdQuery(publicId), cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? playerPublicId = null,
        [FromQuery] string? selectionPublicId = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(new GetPlayerSelectionHistoriesQuery
        {
            Page = page,
            PageSize = pageSize,
            PlayerPublicId = playerPublicId,
            SelectionPublicId = selectionPublicId
        }, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreatePlayerSelectionHistoryCommand command, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{publicId}")]
    public async Task<IActionResult> Update(string publicId, [FromBody] UpdatePlayerSelectionHistoryCommand command, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { PublicId = publicId }, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpDelete("{publicId}")]
    public async Task<IActionResult> Delete(string publicId, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new DeletePlayerSelectionHistoryCommand(publicId), cancellationToken));
    }
}
