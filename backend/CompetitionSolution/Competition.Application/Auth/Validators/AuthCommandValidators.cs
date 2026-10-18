using Competition.Application.Auth.Commands;
using FluentValidation;

namespace Competition.Application.Auth.Validators;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.LoginUser).NotNull();
        When(x => x.LoginUser is not null, () =>
        {
            RuleFor(x => x.LoginUser.Email).NotEmpty().MaximumLength(256).OverridePropertyName("email");
            RuleFor(x => x.LoginUser.Password).NotEmpty().MaximumLength(128).OverridePropertyName("password");
        });
    }
}

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator() => RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(1024);
}

public class LogoutUserCommandValidator : AbstractValidator<LogoutUserCommand>
{
    public LogoutUserCommandValidator() => RuleFor(x => x.token).NotEmpty().MaximumLength(1024);
}
