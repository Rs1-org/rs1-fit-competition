using Competition.Application.Admin.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class UserManagementController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllUsersQuery query, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(query, cancellationToken));

    [HttpPut("{publicId}/role")]
    public async Task<IActionResult> ChangeRole(string publicId, ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        request.PublicId = publicId;
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpPut("{publicId}/active")]
    public async Task<IActionResult> SetActive(string publicId, SetUserActiveCommand request, CancellationToken cancellationToken)
    {
        request.PublicId = publicId;
        return Ok(await mediator.Send(request, cancellationToken));
    }
}
