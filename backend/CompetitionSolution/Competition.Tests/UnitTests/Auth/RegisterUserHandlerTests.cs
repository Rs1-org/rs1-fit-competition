using AutoMapper;
using Competition.Application.Auth.Commands;
using Competition.Application.Auth.DTOs;
using Competition.Application.Auth.Handlers;
using Competition.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Competition.Tests.UnitTests.Auth;

public class RegisterUserHandlerTests
{
  private readonly Mock<UserManager<ApplicationUser>> _users = new(
      Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);
  private readonly Mock<RoleManager<ApplicationRole>> _roles = new(
      Mock.Of<IRoleStore<ApplicationRole>>(), null!, null!, null!, null!);
  private readonly Mock<IMapper> _mapper = new();

  public RegisterUserHandlerTests()
  {
    _ = _users.Object;
    _users.Invocations.Clear();
    _ = _roles.Object;
    _roles.Invocations.Clear();
  }

  private static RegisterUserDto ValidDto() =>
      new("dev@example.invalid", "Strong!aA123", "Dev", "Test", "061111111", "Strong!aA123");

  private void SetupMapper()
  {
    _mapper.Setup(x => x.Map<ApplicationUser>(It.IsAny<RegisterUserDto>()))
        .Returns((RegisterUserDto dto) => new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            UserName = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
        });
  }

  [Fact]
  public async Task SuccessfulRegistration_AssignsUserRole()
  {
    SetupMapper();
    ApplicationUser? captured = null;
    _users.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
        .Callback<ApplicationUser, string>((u, _) => captured = u)
        .ReturnsAsync(IdentityResult.Success);
    _roles.Setup(x => x.RoleExistsAsync("User")).ReturnsAsync(true);
    _users.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User")).ReturnsAsync(IdentityResult.Success);

    var handler = new RegisterUserHandler(_users.Object, _mapper.Object, _roles.Object);
    var result = await handler.Handle(new RegisterUserCommand(ValidDto()), CancellationToken.None);

    result.Success.Should().BeTrue();
    result.UserId.Should().Be(captured!.Id);
    _users.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
    _users.Verify(x => x.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
  }

  [Fact]
  public async Task RoleAssignmentFails_DeletesUserAndReturnsError()
  {
    SetupMapper();
    _users.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
        .ReturnsAsync(IdentityResult.Success);
    _roles.Setup(x => x.RoleExistsAsync("User")).ReturnsAsync(true);
    _users.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
        .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "RoleError", Description = "Could not assign role." }));
    _users.Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

    var handler = new RegisterUserHandler(_users.Object, _mapper.Object, _roles.Object);
    var result = await handler.Handle(new RegisterUserCommand(ValidDto()), CancellationToken.None);

    result.Success.Should().BeFalse();
    result.Errors.Should().ContainKey("role");
    _users.Verify(x => x.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Once);
  }

  [Fact]
  public async Task AfterFailedRoleAssignment_CreateCanSucceedAgain()
  {
    SetupMapper();
    var createCalls = 0;
    _users.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
        .ReturnsAsync(() =>
        {
          createCalls++;
          return IdentityResult.Success;
        });
    _roles.Setup(x => x.RoleExistsAsync("User")).ReturnsAsync(true);
    _users.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
        .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "fail" }));
    _users.Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

    var handler = new RegisterUserHandler(_users.Object, _mapper.Object, _roles.Object);
    (await handler.Handle(new RegisterUserCommand(ValidDto()), CancellationToken.None)).Success.Should().BeFalse();

    _users.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User")).ReturnsAsync(IdentityResult.Success);
    (await handler.Handle(new RegisterUserCommand(ValidDto()), CancellationToken.None)).Success.Should().BeTrue();
    createCalls.Should().Be(2);
  }
}