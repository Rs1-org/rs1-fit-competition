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
    [HttpGet("{publicId}")]
    public async Task<IActionResult> GetById(string publicId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetSelectionManagerHistoryByIdQuery(publicId), cancellationToken));

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? managerPublicId = null,
        [FromQuery] string? selectionPublicId = null,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new GetSelectionManagerHistoriesQuery
        {
            Page = page,
            PageSize = pageSize,
            ManagerPublicId = managerPublicId,
            SelectionPublicId = selectionPublicId
        }, cancellationToken));

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateSelectionManagerHistoryCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{publicId}")]
    public async Task<IActionResult> Update(string publicId, [FromBody] UpdateSelectionManagerHistoryCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command with { PublicId = publicId }, cancellationToken));

    [Authorize(Roles = "Admin,Moderator")]
    [HttpDelete("{publicId}")]
    public async Task<IActionResult> Delete(string publicId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new DeleteSelectionManagerHistoryCommand(publicId), cancellationToken));
}
