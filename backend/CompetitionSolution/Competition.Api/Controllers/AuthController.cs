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
    public async Task<IActionResult> Register([FromBody] RegisterUserDto user)
    {
        var response = await mediator.Send(new RegisterUserCommand(user));

        if (!response.Success)
            return BadRequest(new { message = "Registration failed", errors = response.Errors });

        return Ok(new { userId = response.UserId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto userDto)
    {
        var loginResponse = await mediator.Send(new LoginUserCommand(userDto));
        return Ok(loginResponse);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> Login([FromBody] string userToken)
    {
        try
        {
            var user = await mediator.Send(new RefreshTokenCommand(userToken));
            return Ok(user);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutUserCommand request)
    {
        await mediator.Send(new LogoutUserCommand(request.token));
        return Ok();
    }
}
