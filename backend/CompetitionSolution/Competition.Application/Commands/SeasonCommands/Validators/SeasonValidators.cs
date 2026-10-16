using Competition.Application.Commands.SeasonCommands.Create;
using Competition.Application.Commands.SeasonCommands.Delete;
using Competition.Application.Commands.SeasonCommands.Update;
using Competition.Application.Common.Validation;
using FluentValidation;

namespace Competition.Application.Commands.SeasonCommands.Validators;

public class CreateSeasonCommandValidator : AbstractValidator<CreateSeasonCommand>
{
    public CreateSeasonCommandValidator()
    {
        RuleFor(x => x.CompetitionPublicId)
            .MustBeValidPublicId();
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);
        RuleFor(x => x.StartDate)
            .NotEmpty();
        RuleFor(x => x.EndDate)
            .NotEmpty()
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after the start date.");
    }
}

public class UpdateSeasonCommandValidator : AbstractValidator<UpdateSeasonCommand>
{
    public UpdateSeasonCommandValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty();
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);
        RuleFor(x => x.StartDate)
            .NotEmpty();
        RuleFor(x => x.EndDate)
            .NotEmpty()
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after the start date.");
    }
}

public class DeleteSeasonCommandValidator : AbstractValidator<DeleteSeasonCommand>
{
    public DeleteSeasonCommandValidator()
    {
        RuleFor(x => x.PublicId)
            .MustBeValidPublicId();
    }
}
