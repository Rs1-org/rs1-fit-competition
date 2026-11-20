using Competition.Application.Auth.Handlers;
using Competition.Application.Auth.Commands;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Competition.Tests.UnitTests.Auth;

public class RevokeAllRefreshTokensTests
{
  private static RefreshTokenService CreateService(out AppDbContext context)
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(nameof(RevokeAllRefreshTokensTests) + Guid.NewGuid())
        .Options;
    context = new AppDbContext(options);
    return new RefreshTokenService(context);
  }

  [Fact]
  public async Task RevokeAllUserRefreshTokensAsync_RevokesActiveTokensOnly()
  {
    var service = CreateService(out var context);
    var userId = Guid.NewGuid();
    context.RefreshTokens.AddRange(
        new RefreshToken { UserId = userId, Token = "active-1", Expires = DateTime.UtcNow.AddDays(1) },
        new RefreshToken { UserId = userId, Token = "active-2", Expires = DateTime.UtcNow.AddDays(2) },
        new RefreshToken
        {
            UserId = userId, Token = "already-revoked", Expires = DateTime.UtcNow.AddDays(1),
            IsRevoked = true, RevokedAt = DateTime.UtcNow.AddHours(-1),
        },
        new RefreshToken { UserId = userId, Token = "expired", Expires = DateTime.UtcNow.AddDays(-1) });
    await context.SaveChangesAsync();

    await service.RevokeAllUserRefreshTokensAsync(userId, CancellationToken.None);

    var tokens = await context.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();
    tokens.Where(t => t.Token is "active-1" or "active-2").Should().OnlyContain(t => t.IsRevoked);
    tokens.Single(t => t.Token == "already-revoked").IsRevoked.Should().BeTrue();
    tokens.Single(t => t.Token == "expired").IsRevoked.Should().BeFalse();
  }

  [Fact]
  public async Task AfterPasswordChangeFlow_RefreshWithOldTokenFails()
  {
    var service = CreateService(out var context);
    var userId = Guid.NewGuid();
    context.RefreshTokens.Add(new RefreshToken
    {
        UserId = userId,
        Token = "session-refresh",
        Expires = DateTime.UtcNow.AddDays(7),
    });
    await context.SaveChangesAsync();
    await service.RevokeAllUserRefreshTokensAsync(userId, CancellationToken.None);

    var tokenStore = await service.GetUserAsync("session-refresh", CancellationToken.None);
    var users = new Mock<UserManager<ApplicationUser>>(
        Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);
    _ = users.Object;
    users.Invocations.Clear();

    var handler = new RefreshTokenHandler(service, users.Object, Mock.Of<Competition.Application.Auth.Interfaces.IJwtTokenGenerator>());
    var act = () => handler.Handle(new RefreshTokenCommand("session-refresh"), CancellationToken.None);

    await act.Should().ThrowAsync<UnauthorizedAccessException>();
    tokenStore!.IsRevoked.Should().BeTrue();
  }
}