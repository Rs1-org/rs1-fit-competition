using Competition.Application.Commands.ManagerCommands.Create;
using Competition.Application.Helpers;
using FluentValidation;

namespace Competition.Application.Commands.ManagerCommands.Create;

public class CreateManagerCommandValidator : AbstractValidator<CreateManagerCommand>
{
    public CreateManagerCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .Must(PersonValidationHelper.BeValidDate)
            .WithMessage("Date of birth must be a valid date (yyyy-MM-dd).");
        RuleFor(x => x.CareerStart)
            .Must(PersonValidationHelper.BeValidOptionalDate)
            .WithMessage("Career start must be a valid date (yyyy-MM-dd).");
    }
}
