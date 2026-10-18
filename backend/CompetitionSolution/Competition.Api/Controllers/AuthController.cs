using Competition.Application.Auth.Commands;
using Competition.Application.Auth.DTOs;
using Competition.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Competition.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto user, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new RegisterUserCommand(user), cancellationToken);

        if (!response.Success)
            return BadRequest(new { message = "Registration failed", errors = response.Errors });

        return Ok(new { userId = response.UserId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto userDto, CancellationToken cancellationToken)
    {
        var loginResponse = await mediator.Send(new LoginUserCommand(userDto), cancellationToken);
        return Ok(loginResponse);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> Refresh([FromBody] string userToken, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new RefreshTokenCommand(userToken), cancellationToken));
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new LogoutUserCommand(request.Token), cancellationToken);
        return Ok();
    }
}
