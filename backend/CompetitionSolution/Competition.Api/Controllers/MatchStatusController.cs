using Competition.Application.Queries.Match.GetStatuses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchStatusController : ControllerBase
{
    private readonly IMediator _mediator;

    public MatchStatusController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllMatchStatusesQuery { Page = page, PageSize = pageSize }, cancellationToken);
        return Ok(result);
    }
}
