using Competition.Application.DTOs;
using Competition.Domain.Entities;

namespace Competition.Application.Auth.Interfaces;

public interface IRefreshTokenService
{
    string GenerateRefreshToken();
    Task SaveRefreshTokenAsync(Guid userId, string refreshToken, DateTime expires, CancellationToken cancellationToken = default);
    Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetUserAsync(string refreshToken, CancellationToken cancellationToken = default);
    /// <summary>Revokes the current token and stores the replacement in one database transaction.</summary>
    Task RotateRefreshTokenAsync(Guid userId, string currentToken, string newToken, DateTime newExpires, CancellationToken cancellationToken = default);
}
