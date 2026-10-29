using Competition.Application.Commands.StatisticCommands;
using Competition.Application.Queries.Statistics;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/statistic")]
public class StatisticController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetStatisticsQuery query, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(query, cancellationToken));

    [HttpGet("charts")]
    public async Task<IActionResult> GetCharts([FromQuery] GetStatisticChartsQuery query, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(query, cancellationToken));

    [HttpGet("{publicId}")]
    public async Task<IActionResult> Get(string publicId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetStatisticQuery(publicId), cancellationToken));

    [HttpGet("match/{matchPublicId}")]
    public async Task<IActionResult> GetForMatch(string matchPublicId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetMatchStatisticQuery(matchPublicId), cancellationToken));

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateStatisticCommand request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(request, cancellationToken));

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{publicId}")]
    public async Task<IActionResult> Update(string publicId, UpdateStatisticCommand request, CancellationToken cancellationToken)
    {
        request.PublicId = publicId;
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{publicId}")]
    public async Task<IActionResult> Delete(string publicId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new DeleteStatisticCommand(publicId), cancellationToken));
}
