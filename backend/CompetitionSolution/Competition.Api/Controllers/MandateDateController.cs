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
    [HttpGet("{publicId}")]
    public async Task<IActionResult> GetById(string publicId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetMandateDateByIdQuery(publicId), cancellationToken));

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? playerPublicId = null,
        [FromQuery] string? managerPublicId = null,
        [FromQuery] string? selectionPublicId = null,
        [FromQuery] string? playerSelectionHistoryPublicId = null,
        [FromQuery] string? selectionManagerHistoryPublicId = null,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new GetMandateDatesQuery
        {
            Page = page,
            PageSize = pageSize,
            PlayerPublicId = playerPublicId,
            ManagerPublicId = managerPublicId,
            SelectionPublicId = selectionPublicId,
            PlayerSelectionHistoryPublicId = playerSelectionHistoryPublicId,
            SelectionManagerHistoryPublicId = selectionManagerHistoryPublicId
        }, cancellationToken));

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateMandateDateCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{publicId}")]
    public async Task<IActionResult> Update(string publicId, [FromBody] UpdateMandateDateCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command with { PublicId = publicId }, cancellationToken));

    [Authorize(Roles = "Admin,Moderator")]
    [HttpDelete("{publicId}")]
    public async Task<IActionResult> Delete(string publicId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new DeleteMandateDateCommand(publicId), cancellationToken));
}
