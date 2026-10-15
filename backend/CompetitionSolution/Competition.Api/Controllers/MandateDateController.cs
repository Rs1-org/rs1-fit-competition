using Competition.Application.Commands.MandateDateCommands;
using Competition.Application.Queries.MandateDateQueries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[Route("api/mandate-date")]
[ApiController]
public class MandateDateController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetMandateDateByIdQuery(id), cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? playerId = null,
        [FromQuery] Guid? managerId = null,
        [FromQuery] Guid? selectionId = null,
        [FromQuery] Guid? playerSelectionHistoryId = null,
        [FromQuery] Guid? selectionManagerHistoryId = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(new GetMandateDatesQuery
        {
            Page = page,
            PageSize = pageSize,
            PlayerId = playerId,
            ManagerId = managerId,
            SelectionId = selectionId,
            PlayerSelectionHistoryId = playerSelectionHistoryId,
            SelectionManagerHistoryId = selectionManagerHistoryId
        }, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateMandateDateCommand command, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMandateDateCommand command, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { Id = id }, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new DeleteMandateDateCommand(id), cancellationToken));
    }
}
