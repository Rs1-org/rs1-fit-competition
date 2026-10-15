using Competition.Application.Auth.Settings;
using FluentAssertions;
using Xunit;

namespace Competition.Tests.UnitTests.Auth;

public class SettingsCommandValidatorTests
{
    private readonly UpdateProfileCommandValidator _profile = new();
    private readonly ChangePasswordCommandValidator _password = new();
    private readonly UpdatePreferencesCommandValidator _preferences = new();

    [Fact]
    public void Profile_RejectsEmptyNameAndInvalidEmail()
    {
        var result = _profile.Validate(new UpdateProfileCommand("", "Smailagic", "not-an-email"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "FirstName");
        result.Errors.Should().Contain(error => error.PropertyName == "Email");
    }

    [Fact]
    public void Profile_AcceptsACompleteNameAndEmail()
    {
        var result = _profile.Validate(new UpdateProfileCommand("Edin", "Smailagic", "edin@example.com"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Password_RejectsAWeakPasswordAndAMismatch()
    {
        var result = _password.Validate(new ChangePasswordCommand("Old1!", "short", "other"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "NewPassword");
        result.Errors.Should().Contain(error => error.ErrorMessage == "Passwords do not match.");
    }

    [Fact]
    public void Password_AcceptsAMatchingStrongPassword()
    {
        var result = _password.Validate(new ChangePasswordCommand("Old1!", "NewPass1!", "NewPass1!"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Preferences_RejectAnUnknownTheme()
    {
        var result = _preferences.Validate(new UpdatePreferencesCommand("blue", "en", "dd.MM.yyyy"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.ErrorMessage == "Theme must be light or dark.");
    }

    [Fact]
    public void Preferences_AcceptTheRegionalDateFormat()
    {
        var result = _preferences.Validate(new UpdatePreferencesCommand("dark", "bs", "dd.MM.yyyy"));

        result.IsValid.Should().BeTrue();
    }
}
