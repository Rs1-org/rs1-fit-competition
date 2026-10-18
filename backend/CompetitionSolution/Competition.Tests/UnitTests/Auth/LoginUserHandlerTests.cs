using Competition.Application.Auth.Commands;
using Competition.Application.Auth.DTOs;
using Competition.Application.Auth.Handlers;
using Competition.Application.Exeptions;
using Competition.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Competition.Tests.UnitTests.Auth;

public class LoginUserHandlerTests : AuthUnitTestBase
{
    [Fact]
    public async Task WrongPassword_DoesNotIssueTokens()
    {
        var user = new ApplicationUser { Email = "dev@example.invalid" };
        Users.Setup(x => x.FindByNameAsync(user.Email)).ReturnsAsync(user);
        Users.Setup(x => x.CheckPasswordAsync(user, "wrong")).ReturnsAsync(false);
        var handler = new LoginUserHandler(Users.Object, Jwt.Object, Tokens.Object);

        var act = () => handler.Handle(new LoginUserCommand(new LoginUserDto(user.Email, "wrong")), default);

        await act.Should().ThrowAsync<LoginFailedException>();
        Jwt.VerifyNoOtherCalls();
        Tokens.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ValidCredentials_ReturnTokensAndRoles_AndForwardCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var ct = cancellation.Token;
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "dev@example.invalid", FirstName = "Dev", LastName = "Test" };
        IList<string> roles = new List<string> { "Moderator" };
        Users.Setup(x => x.FindByNameAsync(user.Email)).ReturnsAsync(user);
        Users.Setup(x => x.CheckPasswordAsync(user, "valid")).ReturnsAsync(true);
        Users.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(roles);
        Jwt.Setup(x => x.GenerateJwtToken(user, roles)).Returns("new-access");
        Tokens.Setup(x => x.GenerateRefreshToken()).Returns("new-refresh");
        Tokens.Setup(x => x.SaveRefreshTokenAsync(user.Id, "new-refresh", It.IsAny<DateTime>(), ct)).Returns(Task.CompletedTask);
        var handler = new LoginUserHandler(Users.Object, Jwt.Object, Tokens.Object);

        var result = await handler.Handle(new LoginUserCommand(new LoginUserDto(user.Email, "valid")), ct);

        result.AccessToken.Should().Be("new-access");
        result.RefreshToken.Should().Be("new-refresh");
        result.Roles.Should().Equal("Moderator");
        result.User.Email.Should().Be(user.Email);
        Tokens.Verify(x => x.SaveRefreshTokenAsync(user.Id, "new-refresh", It.IsAny<DateTime>(), ct), Times.Once);
    }

    [Fact]
    public async Task AlreadyCancelledLogin_DoesNotAccessIdentityOrIssueTokens()
    {
        var handler = new LoginUserHandler(Users.Object, Jwt.Object, Tokens.Object);
        var act = () => handler.Handle(new LoginUserCommand(new LoginUserDto("dev@example.invalid", "valid")), new CancellationToken(true));
        await act.Should().ThrowAsync<OperationCanceledException>();
        Users.VerifyNoOtherCalls();
        Jwt.VerifyNoOtherCalls();
        Tokens.VerifyNoOtherCalls();
    }
}
