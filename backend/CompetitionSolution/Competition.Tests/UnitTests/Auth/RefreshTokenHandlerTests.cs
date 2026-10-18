using Competition.Application.Auth.Commands;
using Competition.Application.Auth.Handlers;
using Competition.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Competition.Tests.UnitTests.Auth;

public class RefreshTokenHandlerTests : AuthUnitTestBase
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task RevokedOrExpiredToken_IsRejectedWithoutIssuingNewTokens(bool revoked, bool expired)
    {
        using var cancellation = new CancellationTokenSource();
        var ct = cancellation.Token;
        Tokens.Setup(x => x.GetUserAsync("old", ct)).ReturnsAsync(new RefreshToken
        {
            Token = "old", IsRevoked = revoked, Expires = DateTime.UtcNow.AddDays(expired ? -1 : 1),
        });
        var handler = new RefreshTokenHandler(Tokens.Object, Users.Object, Jwt.Object);

        var act = () => handler.Handle(new RefreshTokenCommand("old"), ct);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        Tokens.Verify(x => x.GetUserAsync("old", ct), Times.Once);
        Tokens.VerifyNoOtherCalls();
        Users.VerifyNoOtherCalls();
        Jwt.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ValidToken_RotatesAndRevokesOldToken_WithRequestCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var ct = cancellation.Token;
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "dev@example.invalid" };
        IList<string> roles = new List<string> { "User" };
        Tokens.Setup(x => x.GetUserAsync("old", ct)).ReturnsAsync(new RefreshToken
        {
            Token = "old", UserId = user.Id, Expires = DateTime.UtcNow.AddDays(1),
        });
        Users.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        Users.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(roles);
        Jwt.Setup(x => x.GenerateJwtToken(user, roles)).Returns("new-access");
        Tokens.Setup(x => x.GenerateRefreshToken()).Returns("new-refresh");
        Tokens.Setup(x => x.SaveRefreshTokenAsync(user.Id, "new-refresh", It.IsAny<DateTime>(), ct)).Returns(Task.CompletedTask);
        Tokens.Setup(x => x.RevokeTokenAsync("old", ct)).Returns(Task.CompletedTask);
        var handler = new RefreshTokenHandler(Tokens.Object, Users.Object, Jwt.Object);

        var result = await handler.Handle(new RefreshTokenCommand("old"), ct);

        result.AccessToken.Should().Be("new-access");
        result.RefreshToken.Should().Be("new-refresh");
        result.Roles.Should().Equal("User");
        Tokens.Verify(x => x.RevokeTokenAsync("old", ct), Times.Once);
        Tokens.Verify(x => x.SaveRefreshTokenAsync(user.Id, "new-refresh", It.IsAny<DateTime>(), ct), Times.Once);
    }

    [Fact]
    public async Task Logout_ForwardsCancellationToRevocation()
    {
        using var cancellation = new CancellationTokenSource();
        var ct = cancellation.Token;
        Tokens.Setup(x => x.RevokeTokenAsync("old", ct)).Returns(Task.CompletedTask);
        var handler = new LogoutUserHandler(Tokens.Object);
        (await handler.Handle(new LogoutUserCommand("old"), ct)).Should().BeTrue();
        Tokens.Verify(x => x.RevokeTokenAsync("old", ct), Times.Once);
    }
}
