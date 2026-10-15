using Competition.Application.Commands.SelectionManagerHistoryCommands;
using Competition.Application.Queries.SelectionManagerHistoryQueries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[Route("api/selection-manager-history")]
[ApiController]
public class SelectionManagerHistoryController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetSelectionManagerHistoryByIdQuery(id), cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? managerId = null,
        [FromQuery] Guid? selectionId = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(new GetSelectionManagerHistoriesQuery
        {
            Page = page,
            PageSize = pageSize,
            ManagerId = managerId,
            SelectionId = selectionId
        }, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateSelectionManagerHistoryCommand command, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSelectionManagerHistoryCommand command, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { Id = id }, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new DeleteSelectionManagerHistoryCommand(id), cancellationToken));
    }
}
