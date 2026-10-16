using Competition.Application.Commands.RoundCommands.Create;
using Competition.Application.Commands.RoundCommands.Delete;
using Competition.Application.Commands.RoundCommands.Update;
using Competition.Application.Common.Validation;
using FluentValidation;

namespace Competition.Application.Commands.RoundCommands.Validators;

public class CreateRoundCommandValidator : AbstractValidator<CreateRoundCommand>
{
    public CreateRoundCommandValidator()
    {
        RuleFor(x => x.SeasonPublicId)
            .MustBeValidPublicId();
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);
        RuleFor(x => x.Number)
            .InclusiveBetween(1, 100);
    }
}

public class UpdateRoundCommandValidator : AbstractValidator<UpdateRoundCommand>
{
    public UpdateRoundCommandValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty();
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);
        RuleFor(x => x.Number)
            .InclusiveBetween(1, 100);
    }
}

public class DeleteRoundCommandValidator : AbstractValidator<DeleteRoundCommand>
{
    public DeleteRoundCommandValidator()
    {
        RuleFor(x => x.PublicId)
            .MustBeValidPublicId();
    }
}
