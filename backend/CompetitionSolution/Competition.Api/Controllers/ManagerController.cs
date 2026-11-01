using Competition.Application.DTOs.PersonDto;
using Competition.Application.Commands.ManagerCommands.Create;
using Competition.Application.Commands.ManagerCommands.Delete;
using Competition.Application.Commands.ManagerCommands.Update;
using Competition.Application.Queries.ManagerQueries.GetAll;
using Competition.Application.Queries.ManagerQueries.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[Route("api/manager")]
[ApiController]
public class ManagerController(IMediator mediator) : ControllerBase
{
    [HttpGet("{publicId}")]
    public async Task<IActionResult> GetById(string publicId, CancellationToken ct)
    {
        var manager = await mediator.Send(new GetManagerByIdQuery(publicId), ct);
        if (manager == null)
            throw new KeyNotFoundException($"Manager with public id {publicId} not found");

        return Ok(manager);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] int? ageMin = null,
        [FromQuery] int? ageMax = null,
        [FromQuery] DateOnly? careerStartFrom = null,
        [FromQuery] DateOnly? careerStartTo = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(new GetAllManagersQuery
        {
            Page = page,
            PageSize = pageSize,
            Name = name,
            AgeMin = ageMin,
            AgeMax = ageMax,
            CareerStartFrom = careerStartFrom,
            CareerStartTo = careerStartTo
        }, cancellationToken));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    public async Task<IActionResult> AddManager([FromBody] ManagerDto dto, CancellationToken ct)
    {
        var created = await mediator.Send(new CreateManagerCommand(dto), ct);
        return Ok(created);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{publicId}")]
    public async Task<IActionResult> UpdateManager(string publicId, [FromBody] ManagerDto dto, CancellationToken ct)
    {
        var updated = await mediator.Send(new UpdateManagerCommand(publicId, dto), ct);
        return Ok(updated);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpDelete("{publicId}")]
    public async Task<IActionResult> DeleteManager(string publicId, CancellationToken ct)
    {
        var deletedId = await mediator.Send(new DeleteManagerCommand(publicId), ct);
        return Ok(deletedId);
    }

}
