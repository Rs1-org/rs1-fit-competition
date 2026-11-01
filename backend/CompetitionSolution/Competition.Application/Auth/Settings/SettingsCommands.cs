using Competition.Application.Auth.Interfaces;
using Competition.Application.DTOs;
using Competition.Application.Payments;
using Competition.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Competition.Application.Auth.Settings;

public record SettingsDto(
    string FirstName,
    string LastName,
    string Email,
    string Theme,
    string Language,
    string DateFormat);

public record GetSettingsQuery : IRequest<SettingsDto>;

public class GetSettingsQueryHandler(UserManager<ApplicationUser> users, ICurrentUser current, IUserPreferenceStore preferences)
    : IRequestHandler<GetSettingsQuery, SettingsDto>
{
    public async Task<SettingsDto> Handle(GetSettingsQuery request, CancellationToken cancellationToken)
    {
        var user = await RequireUser();
        var preference = await preferences.GetAsync(user.Id, cancellationToken);
        return Map(user, preference);
    }

    private async Task<ApplicationUser> RequireUser()
    {
        return await users.FindByIdAsync(current.UserId.ToString())
            ?? throw new KeyNotFoundException("User was not found.");
    }

    internal static SettingsDto Map(ApplicationUser user, UserPreferenceData preference) => new(
        user.FirstName,
        user.LastName,
        user.Email ?? string.Empty,
        preference.Theme,
        preference.Language,
        preference.DateFormat);
}

public record UpdateProfileCommand(string FirstName, string LastName, string Email) : IRequest<UserDto>;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
    }
}

public class UpdateProfileCommandHandler(UserManager<ApplicationUser> users, ICurrentUser current)
    : IRequestHandler<UpdateProfileCommand, UserDto>
{
    public async Task<UserDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(current.UserId.ToString())
            ?? throw new KeyNotFoundException("User was not found.");

        var email = request.Email.Trim();
        var existing = await users.FindByEmailAsync(email);
        if (existing is not null && existing.Id != user.Id)
            throw new ValidationException(new[] { new ValidationFailure("Email", "Email is already in use.") });

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        IdentityGuard.ThrowIfFailed(await users.SetEmailAsync(user, email));
        IdentityGuard.ThrowIfFailed(await users.SetUserNameAsync(user, email));
        IdentityGuard.ThrowIfFailed(await users.UpdateAsync(user));

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            PremiumUntil = PremiumPlan.Format(user.PremiumUntil)
        };
    }
}

public record ChangePasswordCommand(string CurrentPassword, string NewPassword, string ConfirmPassword) : IRequest<Unit>;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(6)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a symbol.");
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword)
            .WithMessage("Passwords do not match.");
    }
}

public class ChangePasswordCommandHandler(UserManager<ApplicationUser> users, ICurrentUser current)
    : IRequestHandler<ChangePasswordCommand, Unit>
{
    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(current.UserId.ToString())
            ?? throw new KeyNotFoundException("User was not found.");

        var result = await users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            throw new ValidationException(new[] { new ValidationFailure("CurrentPassword", "Current password is incorrect.") });

        return Unit.Value;
    }
}

public record UpdatePreferencesCommand(string Theme, string Language, string DateFormat) : IRequest<UserPreferenceData>;

public class UpdatePreferencesCommandValidator : AbstractValidator<UpdatePreferencesCommand>
{
    public static readonly string[] Themes = ["light", "dark"];
    public static readonly string[] Languages = ["bs", "en"];
    public static readonly string[] DateFormats = ["dd.MM.yyyy", "yyyy-MM-dd"];

    public UpdatePreferencesCommandValidator()
    {
        RuleFor(x => x.Theme).Must(theme => Themes.Contains(theme)).WithMessage("Theme must be light or dark.");
        RuleFor(x => x.Language).Must(language => Languages.Contains(language)).WithMessage("Language must be bs or en.");
        RuleFor(x => x.DateFormat).Must(format => DateFormats.Contains(format)).WithMessage("Date format is not supported.");
    }
}

public class UpdatePreferencesCommandHandler(ICurrentUser current, IUserPreferenceStore preferences)
    : IRequestHandler<UpdatePreferencesCommand, UserPreferenceData>
{
    public async Task<UserPreferenceData> Handle(UpdatePreferencesCommand request, CancellationToken cancellationToken)
    {
        var preference = new UserPreferenceData(request.Theme, request.Language, request.DateFormat);
        await preferences.SaveAsync(current.UserId, preference, cancellationToken);
        return preference;
    }
}

internal static class IdentityGuard
{
    public static void ThrowIfFailed(IdentityResult result)
    {
        if (result.Succeeded) return;
        throw new ValidationException(result.Errors.Select(error => new ValidationFailure(error.Code, error.Description)));
    }
}
