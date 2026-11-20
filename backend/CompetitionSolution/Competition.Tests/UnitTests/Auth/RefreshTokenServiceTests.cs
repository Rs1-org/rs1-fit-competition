using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Competition.Tests.UnitTests.Auth;

public class RefreshTokenServiceTests
{
    private static RefreshTokenService CreateService(out AppDbContext context)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nameof(RefreshTokenServiceTests) + Guid.NewGuid())
            .Options;
        context = new AppDbContext(options);
        return new RefreshTokenService(context);
    }

    [Fact]
    public async Task RotateRefreshTokenAsync_RevokesOldTokenAndStoresNewOne()
    {
        var service = CreateService(out var context);
        var userId = Guid.NewGuid();
        context.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            Token = "old",
            Expires = DateTime.UtcNow.AddDays(1),
        });
        await context.SaveChangesAsync();

        await service.RotateRefreshTokenAsync(userId, "old", "new", DateTime.UtcNow.AddDays(7), CancellationToken.None);

        var tokens = await context.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();
        tokens.Should().HaveCount(2);
        tokens.Single(t => t.Token == "old").IsRevoked.Should().BeTrue();
        tokens.Single(t => t.Token == "new").IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task RotateRefreshTokenAsync_SecondUseOfOldToken_Rejects()
    {
        var service = CreateService(out var context);
        var userId = Guid.NewGuid();
        context.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            Token = "old",
            Expires = DateTime.UtcNow.AddDays(1),
        });
        await context.SaveChangesAsync();

        await service.RotateRefreshTokenAsync(userId, "old", "new-1", DateTime.UtcNow.AddDays(7), CancellationToken.None);

        var act = () => service.RotateRefreshTokenAsync(userId, "old", "new-2", DateTime.UtcNow.AddDays(7), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        (await context.RefreshTokens.CountAsync(t => t.UserId == userId && !t.IsRevoked)).Should().Be(1);
    }
}