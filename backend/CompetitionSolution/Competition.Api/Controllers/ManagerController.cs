using Competition.Application.DTOs.PersonDto;
using Competition.Application.Features.ManagerCommands.Create;
using Competition.Application.Features.ManagerCommands.Delete;
using Competition.Application.Features.ManagerCommands.Update;
using Competition.Application.Features.ManagerQueries.GetAll;
using Competition.Application.Features.ManagerQueries.GetById;
using Competition.Application.Features.ManagerQueries.Search;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[Route("api/manager")]
[ApiController]
public class ManagerController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var manager = await mediator.Send(new GetManagerByIdQuery(id));
        if (manager == null)
            throw new KeyNotFoundException($"Manager with id {id} not found");

        return Ok(manager);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await mediator.Send(new GetAllManagersQuery()));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    public async Task<IActionResult> AddManager([FromBody] ManagerDto dto)
    {
        var created = await mediator.Send(new CreateManagerCommand(dto));
        return Ok(created);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateManager(Guid id, [FromBody] ManagerDto dto)
    {
        var updated = await mediator.Send(new UpdateManagerCommand(id, dto));
        return Ok(updated);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteManager(Guid id)
    {
        var deletedId = await mediator.Send(new DeleteManagerCommand(id));
        return Ok(deletedId);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return Ok(await mediator.Send(new GetAllManagersQuery()));

        return Ok(await mediator.Send(new SearchManagersQuery(term)));
    }
}
