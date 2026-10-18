using Competition.Application.Auth.Interfaces;
using Competition.Domain.Entities;
using Competition.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Competition.Tests.UnitTests.Auth;

public abstract class AuthUnitTestBase : BaseUnitTest
{
    protected readonly Mock<UserManager<ApplicationUser>> Users = new(
        Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);
    protected readonly Mock<IRefreshTokenService> Tokens = new(MockBehavior.Strict);
    protected readonly Mock<IJwtTokenGenerator> Jwt = new(MockBehavior.Strict);

    protected AuthUnitTestBase()
    {
        _ = Users.Object; // Construct the lazy proxy before excluding Identity's initial property setters.
        Users.Invocations.Clear();
    }
}
