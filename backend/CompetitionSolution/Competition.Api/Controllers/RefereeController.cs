using Competition.Application.DTOs.PersonDto;
using Competition.Application.Features.RefereeCommands.Create;
using Competition.Application.Features.RefereeCommands.Delete;
using Competition.Application.Features.RefereeCommands.Update;
using Competition.Application.Features.RefereeQueries.GetAll;
using Competition.Application.Features.RefereeQueries.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[Route("api/referee")]
[ApiController]
public class RefereeController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var referee = await mediator.Send(new GetRefereeByIdQuery(id));
        if (referee == null)
            throw new KeyNotFoundException($"Referee with id {id} not found");

        return Ok(referee);
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
        return Ok(await mediator.Send(new GetAllRefereesQuery
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
    public async Task<IActionResult> AddReferee([FromBody] RefereeDto dto)
    {
        var created = await mediator.Send(new CreateRefereeCommand(dto));
        return Ok(created);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReferee(Guid id, [FromBody] RefereeDto dto)
    {
        var updated = await mediator.Send(new UpdateRefereeCommand(id, dto));
        return Ok(updated);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReferee(Guid id)
    {
        var deletedId = await mediator.Send(new DeleteRefereeCommand(id));
        return Ok(deletedId);
    }

}
