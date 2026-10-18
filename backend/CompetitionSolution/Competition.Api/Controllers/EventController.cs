using Competition.Application.Events;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetEventsQuery query, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(query, cancellationToken));
    [HttpGet("lookups")]
    public async Task<IActionResult> Lookups(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetEventLookupsQuery(), cancellationToken));
    [HttpGet("match/{matchPublicId}/context")]
    public async Task<IActionResult> Context(string matchPublicId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetEventContextQuery(matchPublicId), cancellationToken));
    [HttpGet("{publicId}")]
    public async Task<IActionResult> Details(string publicId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetEventQuery(publicId), cancellationToken));
    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateEventCommand request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(request, cancellationToken));
    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{publicId}")]
    public async Task<IActionResult> Update(string publicId, UpdateEventCommand request, CancellationToken cancellationToken)
    {
        request.PublicId = publicId;
        return Ok(await mediator.Send(request, cancellationToken));
    }
    // COMP-41 explicitly allows moderators to delete events as well as add/edit them.
    [Authorize(Roles = "Admin,Moderator")]
    [HttpDelete("{publicId}")]
    public async Task<IActionResult> Delete(string publicId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new DeleteEventCommand(publicId), cancellationToken));
}
