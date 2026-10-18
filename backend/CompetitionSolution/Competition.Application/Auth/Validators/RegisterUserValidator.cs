namespace Competition.Application.Auth.Validators;

using Competition.Application.Auth.Commands;
using Competition.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator(UserManager<ApplicationUser> userManager)
    {
        RuleFor(x => x.UserDto).NotNull();
        When(x => x.UserDto is not null, () =>
        {
            // Email is the Identity username in the existing public registration contract.
            RuleFor(x => x.UserDto.Email).Cascade(CascadeMode.Stop).NotEmpty().EmailAddress().MaximumLength(256)
                .MustAsync(async (email, ct) =>
                {
                    ct.ThrowIfCancellationRequested();
                    var existing = await userManager.FindByEmailAsync(email);
                    ct.ThrowIfCancellationRequested();
                    return existing is null;
                }).WithMessage("This email is already registered.").OverridePropertyName("email");
            RuleFor(x => x.UserDto.Password).NotEmpty().MinimumLength(8).MaximumLength(128)
                .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain a digit.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a special character.")
                .OverridePropertyName("password");
            RuleFor(x => x.UserDto.ConfirmPassword).NotEmpty().Equal(x => x.UserDto.Password)
                .WithMessage("Passwords must match.").OverridePropertyName("confirmPassword");
            RuleFor(x => x.UserDto.FirstName).NotEmpty().MaximumLength(100).OverridePropertyName("firstName");
            RuleFor(x => x.UserDto.LastName).NotEmpty().MaximumLength(100).OverridePropertyName("lastName");
            RuleFor(x => x.UserDto.PhoneNumber).NotEmpty().MaximumLength(30).OverridePropertyName("phoneNumber");
        });
    }
}
