using Competition.Application.Features.PlayerCommands.Create;
using Competition.Application.Helpers;
using FluentValidation;

namespace Competition.Application.Commands.PlayerCommands.Create;

public class CreatePlayerCommandValidator : AbstractValidator<CreatePlayerCommand>
{
    public CreatePlayerCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .Must(PersonValidationHelper.BeValidDate)
            .WithMessage("Date of birth must be a valid date (yyyy-MM-dd).");
        RuleFor(x => x.Height).GreaterThan(0).LessThanOrEqualTo(300);
        RuleFor(x => x.ShirtNumber).InclusiveBetween(1, 99);
        RuleFor(x => x.TransferValue).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PositionId)
            .NotEmpty()
            .When(x => x.SubPositionId.HasValue)
            .WithMessage("Position is required when a subposition is set.");
    }
}
