using Competition.Application.Commands.TransferValueHistoryCommands;
using Competition.Application.Queries.TransferValueHistoryQueries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[Route("api/transfer-value-history")]
[ApiController]
public class TransferValueHistoryController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetTransferValueHistoryByIdQuery(id), cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? playerId = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(new GetTransferValueHistoriesQuery
        {
            Page = page,
            PageSize = pageSize,
            PlayerId = playerId
        }, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateTransferValueHistoryCommand command, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransferValueHistoryCommand command, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { Id = id }, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new DeleteTransferValueHistoryCommand(id), cancellationToken));
    }
}
