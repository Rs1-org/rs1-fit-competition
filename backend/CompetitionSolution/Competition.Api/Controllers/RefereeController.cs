using Competition.Application.DTOs.PersonDto;
using Competition.Application.Features.RefereeCommands.Create;
using Competition.Application.Features.RefereeCommands.Delete;
using Competition.Application.Features.RefereeCommands.Update;
using Competition.Application.Features.RefereeQueries.GetAll;
using Competition.Application.Features.RefereeQueries.GetById;
using Competition.Application.Features.RefereeQueries.Search;
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
    public async Task<IActionResult> GetAll()
    {
        return Ok(await mediator.Send(new GetAllRefereesQuery()));
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddReferee([FromBody] RefereeDto dto)
    {
        var created = await mediator.Send(new CreateRefereeCommand(dto));
        return Ok(created);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReferee(Guid id, [FromBody] RefereeDto dto)
    {
        var updated = await mediator.Send(new UpdateRefereeCommand(id, dto));
        return Ok(updated);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReferee(Guid id)
    {
        var deletedId = await mediator.Send(new DeleteRefereeCommand(id));
        return Ok(deletedId);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return Ok(await mediator.Send(new GetAllRefereesQuery()));

        return Ok(await mediator.Send(new SearchRefereesQuery(term)));
    }
}
