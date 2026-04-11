using Competition.Application.DTOs;
using Competition.Domain.Entities;

namespace Competition.Application.Auth.Interfaces;

public interface IRefreshTokenService
{
    string GenerateRefreshToken();
    Task SaveRefreshTokenAsync(Guid userId, string refreshToken, DateTime expires);
    Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken);
    Task RevokeTokenAsync(string refreshToken);
    Task<RefreshToken> GetUserAsync(string refreshToken);
}