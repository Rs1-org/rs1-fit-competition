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

    public async Task SaveRefreshTokenAsync(Guid userId, string refreshToken, DateTime expires)
    {
        var token = new RefreshToken
        {
            UserId = userId,
            Token = refreshToken,
            Expires = expires,
        };

        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync();
    }

    public async Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
    {
        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == refreshToken);

        if (token == null || token.Expires < DateTime.UtcNow)
            return false;

        return true;
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (token != null)
        {
            context.RefreshTokens.Remove(token);
            await context.SaveChangesAsync();
        }
    }

    public async Task<RefreshToken?> GetUserAsync(string refreshToken)
    {
        return await context.RefreshTokens.FirstOrDefaultAsync(it => it.Token == refreshToken);
    }
}