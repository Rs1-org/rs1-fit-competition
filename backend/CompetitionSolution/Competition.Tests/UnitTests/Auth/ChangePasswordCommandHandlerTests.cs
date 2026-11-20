using Competition.Application.Auth.Interfaces;
using Competition.Application.Auth.Settings;
using Competition.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Competition.Tests.UnitTests.Auth;

public class ChangePasswordCommandHandlerTests
{
  private readonly Mock<UserManager<ApplicationUser>> _users = new(
      Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);
  private readonly Mock<ICurrentUser> _current = new();
  private readonly Mock<IRefreshTokenService> _tokens = new(MockBehavior.Strict);

  public ChangePasswordCommandHandlerTests()
  {
    _ = _users.Object;
    _users.Invocations.Clear();
  }

  [Fact]
  public async Task SuccessfulChange_RevokesAllRefreshTokens()
  {
    var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@test.invalid" };
    _current.Setup(x => x.UserId).Returns(user.Id);
    _users.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
    _users.Setup(x => x.ChangePasswordAsync(user, "OldPass1!", "NewPass1!"))
        .ReturnsAsync(IdentityResult.Success);
    _tokens.Setup(x => x.RevokeAllUserRefreshTokensAsync(user.Id, It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    var handler = new ChangePasswordCommandHandler(_users.Object, _current.Object, _tokens.Object);
    await handler.Handle(new ChangePasswordCommand("OldPass1!", "NewPass1!", "NewPass1!"), CancellationToken.None);

    _tokens.Verify(x => x.RevokeAllUserRefreshTokensAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task WrongCurrentPassword_DoesNotRevokeRefreshTokens()
  {
    var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@test.invalid" };
    _current.Setup(x => x.UserId).Returns(user.Id);
    _users.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
    _users.Setup(x => x.ChangePasswordAsync(user, "wrong", "NewPass1!"))
        .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Bad password." }));

    var handler = new ChangePasswordCommandHandler(_users.Object, _current.Object, _tokens.Object);
    var act = () => handler.Handle(new ChangePasswordCommand("wrong", "NewPass1!", "NewPass1!"), CancellationToken.None);

    await act.Should().ThrowAsync<ValidationException>();
    _tokens.VerifyNoOtherCalls();
  }
}