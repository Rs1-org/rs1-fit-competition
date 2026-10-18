using System.Security.Claims;
using Competition.Application.Reviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewController(IMediator mediator) : ControllerBase
{
    [HttpGet("match/{matchPublicId}")]
    public async Task<IActionResult> Get(string matchPublicId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        Guid? viewer = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var id) ? id : null;
        return Ok(await mediator.Send(new GetMatchReviewsQuery(matchPublicId, page, pageSize, viewer), cancellationToken));
    }
    [Authorize]
    [HttpPost("match/{matchPublicId}")]
    public async Task<IActionResult> Create(string matchPublicId, CreateReviewCommand request, CancellationToken cancellationToken)
    {
        request.MatchPublicId = matchPublicId;
        return Ok(await mediator.Send(request, cancellationToken));
    }
    [Authorize]
    [HttpPut("{publicId}")]
    public async Task<IActionResult> Update(string publicId, UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        request.PublicId = publicId;
        return Ok(await mediator.Send(request, cancellationToken));
    }
    [Authorize]
    [HttpDelete("{publicId}")]
    public async Task<IActionResult> Delete(string publicId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new DeleteReviewCommand(publicId), cancellationToken));
    [Authorize]
    [HttpPut("{publicId}/reaction")]
    public async Task<IActionResult> React(string publicId, SetReviewReactionCommand request, CancellationToken cancellationToken)
    {
        request.PublicId = publicId;
        return Ok(await mediator.Send(request, cancellationToken));
    }
}
