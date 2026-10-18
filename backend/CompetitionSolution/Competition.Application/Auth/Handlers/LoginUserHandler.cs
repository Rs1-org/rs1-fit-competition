using Competition.Application.Auth.Commands;
using Competition.Application.Auth.DTOs;
using Competition.Application.Auth.Interfaces;
using Competition.Application.DTOs;
using Competition.Application.Exeptions;
using Competition.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Competition.Application.Auth.Handlers;

public class LoginUserHandler(UserManager<ApplicationUser> userManager, IJwtTokenGenerator jwtTokenGenerator, IRefreshTokenService refreshTokenService)
    : IRequestHandler<LoginUserCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByNameAsync(request.LoginUser.Email);
        if (user is null || !user.IsActive)
            throw new LoginFailedException("Invalid credentials");

        var isValid = await userManager.CheckPasswordAsync(user, request.LoginUser.Password);
        if (!isValid)
            throw new LoginFailedException("Invalid credentials");

        var roles = await userManager.GetRolesAsync(user);
        cancellationToken.ThrowIfCancellationRequested();
        
        var accessToken = jwtTokenGenerator.GenerateJwtToken(user, roles);
        
        var refreshToken = refreshTokenService.GenerateRefreshToken();
        var refreshExpires = DateTime.UtcNow.AddDays(7);

        await refreshTokenService.SaveRefreshTokenAsync(
            user.Id,
            refreshToken,
            refreshExpires, cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Expires = DateTime.UtcNow.AddHours(1),
            Roles = roles.ToList(),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName!,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber
            }
        };
    }
}
