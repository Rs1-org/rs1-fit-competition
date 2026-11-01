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
    [HttpGet("{publicId}")]
    public async Task<IActionResult> GetById(string publicId, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetTransferValueHistoryByIdQuery(publicId), cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? playerPublicId = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(new GetTransferValueHistoriesQuery
        {
            Page = page,
            PageSize = pageSize,
            PlayerPublicId = playerPublicId
        }, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateTransferValueHistoryCommand command, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{publicId}")]
    public async Task<IActionResult> Update(string publicId, [FromBody] UpdateTransferValueHistoryCommand command, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { PublicId = publicId }, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpDelete("{publicId}")]
    public async Task<IActionResult> Delete(string publicId, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new DeleteTransferValueHistoryCommand(publicId), cancellationToken));
    }
}
