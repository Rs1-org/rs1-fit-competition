using System.Security.Cryptography;
using Competition.Application.Auth.Interfaces;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Competition.Infrastructure.Identity;

public class RefreshTokenService(AppDbContext context) : IRefreshTokenService
{
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task SaveRefreshTokenAsync(Guid userId, string refreshToken, DateTime expires, CancellationToken cancellationToken = default)
    {
        var token = new RefreshToken
        {
            UserId = userId,
            Token = refreshToken,
            Expires = expires,
        };

        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == refreshToken, cancellationToken);

        if (token == null || token.IsRevoked || token.Expires <= DateTime.UtcNow)
            return false;

        return true;
    }

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);

        if (token is { IsRevoked: false })
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<RefreshToken?> GetUserAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await context.RefreshTokens.FirstOrDefaultAsync(it => it.Token == refreshToken, cancellationToken);
    }
}
