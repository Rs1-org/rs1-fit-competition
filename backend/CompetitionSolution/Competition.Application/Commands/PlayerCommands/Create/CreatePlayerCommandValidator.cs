using Competition.Application.Common.Validation;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.FormationEntities;
using FluentValidation;

namespace Competition.Application.Commands.PlayerCommands.Create;

public class CreatePlayerCommandValidator : AbstractValidator<CreatePlayerCommand>
{
    public CreatePlayerCommandValidator(IPublicIdLookup lookup)
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
        RuleFor(x => x.PositionPublicId)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.SubPositionPublicId))
            .WithMessage("Position is required when a subposition is set.");
        RuleFor(x => x.PositionPublicId!).MustReferenceExisting<CreatePlayerCommand, Position>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.PositionPublicId));
        RuleFor(x => x.SubPositionPublicId!).MustReferenceExisting<CreatePlayerCommand, SubPosition>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.SubPositionPublicId));
    }
}
