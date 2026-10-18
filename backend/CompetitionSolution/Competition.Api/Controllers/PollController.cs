using System.Security.Claims;
using Competition.Application.Polls;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/polls")]
public class PollController(IMediator mediator) : ControllerBase
{
    [HttpGet("match/{matchPublicId}")]
    public async Task<IActionResult> Get(string matchPublicId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        Guid? viewer = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var id) ? id : null;
        return Ok(await mediator.Send(new GetMatchPollsQuery(matchPublicId, page, pageSize, viewer), cancellationToken));
    }
    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost("match/{matchPublicId}")]
    public async Task<IActionResult> Create(string matchPublicId, CreatePollCommand request, CancellationToken cancellationToken)
    {
        request.MatchPublicId = matchPublicId; return Ok(await mediator.Send(request, cancellationToken));
    }
    [Authorize]
    [HttpPost("{publicId}/vote")]
    public async Task<IActionResult> Vote(string publicId, VotePollCommand request, CancellationToken cancellationToken)
    {
        request.PollPublicId = publicId; return Ok(await mediator.Send(request, cancellationToken));
    }
}
