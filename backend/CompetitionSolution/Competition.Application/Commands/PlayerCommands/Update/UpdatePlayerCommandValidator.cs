using Competition.Application.Common.Validation;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.FormationEntities;
using FluentValidation;

namespace Competition.Application.Commands.PlayerCommands.Update;

public class UpdatePlayerCommandValidator : AbstractValidator<UpdatePlayerCommand>
{
    public UpdatePlayerCommandValidator(IPublicIdLookup lookup)
    {
        RuleFor(x => x.PublicId).MustBeValidPublicId();
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
        RuleFor(x => x.PositionPublicId!).MustReferenceExisting<UpdatePlayerCommand, Position>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.PositionPublicId));
        RuleFor(x => x.SubPositionPublicId!).MustReferenceExisting<UpdatePlayerCommand, SubPosition>(lookup)
            .When(x => !string.IsNullOrWhiteSpace(x.SubPositionPublicId));
    }
}
