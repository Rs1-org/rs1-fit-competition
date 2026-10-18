using System.Runtime.Serialization.Json;
using Competition.Application.Auth.Commands;
using Competition.Application.Auth.DTOs;
using Competition.Application.Auth.Interfaces;
using Competition.Application.DTOs;
using Competition.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Competition.Application.Auth.Handlers;

public class RefreshTokenHandler(IRefreshTokenService refreshTokenService, UserManager<ApplicationUser> userManager, IJwtTokenGenerator jwtTokenGenerator):IRequestHandler<RefreshTokenCommand,LoginResponse>
{
    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenUser = await refreshTokenService.GetUserAsync(request.RefreshToken);
        if (tokenUser == null || tokenUser.IsRevoked || tokenUser.Expires < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }
        var user = await userManager.FindByIdAsync(tokenUser.UserId.ToString());
        if (user == null)
            throw new UnauthorizedAccessException("User not found");
        
        var roles = await userManager.GetRolesAsync(user);
        
        var newAccessToken = jwtTokenGenerator.GenerateJwtToken(user, roles);
        
        var newRefreshToken = refreshTokenService.GenerateRefreshToken();
        var expires = DateTime.UtcNow.AddDays(7);
        
        await refreshTokenService.SaveRefreshTokenAsync(user.Id, newRefreshToken, expires);
        
        await refreshTokenService.RevokeTokenAsync(request.RefreshToken);
        
        return new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            Expires = DateTime.UtcNow.AddHours(1),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName!,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber
            },
            Roles = roles.ToList()
        };
    }
}
