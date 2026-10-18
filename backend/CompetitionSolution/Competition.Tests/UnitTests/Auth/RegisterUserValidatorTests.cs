using Competition.Application.Auth.Commands;
using Competition.Application.Auth.DTOs;
using Competition.Application.Auth.Validators;
using Competition.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Competition.Tests.UnitTests.Auth;

public class RegisterUserValidatorTests : AuthUnitTestBase
{
    private static RegisterUserDto Valid() => new("dev@example.invalid", "Strong!aA123", "Dev", "Test", "061111111", "Strong!aA123");

    [Fact]
    public async Task CompleteRegistration_WithUnusedEmail_Passes()
    {
        Users.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        var validator = new RegisterUserValidator(Users.Object);
        (await validator.ValidateAsync(new RegisterUserCommand(Valid()))).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid", "Strong!aA123", "Strong!aA123", "email")]
    [InlineData("dev@example.invalid", "weak", "weak", "password")]
    [InlineData("dev@example.invalid", "Strong!aA123", "different", "confirmPassword")]
    public async Task InvalidRegistration_ReportsTheActionableField(string email, string password, string confirmation, string field)
    {
        var validator = new RegisterUserValidator(Users.Object);
        var dto = Valid() with { Email = email, Password = password, ConfirmPassword = confirmation };
        var result = await validator.ValidateAsync(new RegisterUserCommand(dto));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == field);
    }

    [Fact]
    public async Task ExistingEmail_IsRejectedBeforeCreatingAUser()
    {
        Users.Setup(x => x.FindByEmailAsync("dev@example.invalid")).ReturnsAsync(new ApplicationUser());
        var validator = new RegisterUserValidator(Users.Object);
        var result = await validator.ValidateAsync(new RegisterUserCommand(Valid()));
        result.Errors.Should().Contain(x => x.PropertyName == "email" && x.ErrorMessage.Contains("already registered"));
    }

    [Fact]
    public async Task BlankNamesAndPhone_AreRejected()
    {
        var validator = new RegisterUserValidator(Users.Object);
        var dto = Valid() with { FirstName = " ", LastName = "", PhoneNumber = "" };
        var result = await validator.ValidateAsync(new RegisterUserCommand(dto));
        result.Errors.Select(x => x.PropertyName).Should().Contain(["firstName", "lastName", "phoneNumber"]);
    }
}
