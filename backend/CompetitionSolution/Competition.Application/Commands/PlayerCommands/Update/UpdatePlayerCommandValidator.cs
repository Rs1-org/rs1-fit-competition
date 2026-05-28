using Competition.Application.Features.PlayerCommands.Update;
using Competition.Application.Helpers;
using FluentValidation;

namespace Competition.Application.Commands.PlayerCommands.Update;

public class UpdatePlayerCommandValidator : AbstractValidator<UpdatePlayerCommand>
{
    public UpdatePlayerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .Must(PersonValidationHelper.BeValidDate)
            .WithMessage("Date of birth must be a valid date (yyyy-MM-dd).");
        RuleFor(x => x.Height).GreaterThan(0).LessThanOrEqualTo(300);
        RuleFor(x => x.ShirtNumber).InclusiveBetween(1, 99);
        RuleFor(x => x.TransferValue).GreaterThanOrEqualTo(0);
    }
}
