using Competition.Application.Commands.CompetitionCommands.Create;
using Competition.Application.Commands.CompetitionCommands.Delete;
using Competition.Application.Commands.CompetitionCommands.Update;
using FluentValidation;

namespace Competition.Application.Commands.CompetitionCommands.Validators;

public class CreateCompetitionCommandValidator : AbstractValidator<CreateCompetitionCommand>
{
    public CreateCompetitionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);
        RuleFor(x => x.ShortName)
            .MaximumLength(20);
        RuleFor(x => x.Description)
            .MaximumLength(1000);
        RuleFor(x => x.CountryId)
            .NotEqual(Guid.Empty)
            .When(x => x.CountryId.HasValue);
    }
}

public class UpdateCompetitionCommandValidator : AbstractValidator<UpdateCompetitionCommand>
{
    public UpdateCompetitionCommandValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty();
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);
        RuleFor(x => x.ShortName)
            .MaximumLength(20);
        RuleFor(x => x.Description)
            .MaximumLength(1000);
        RuleFor(x => x.CountryId)
            .NotEqual(Guid.Empty)
            .When(x => x.CountryId.HasValue);
    }
}

public class DeleteCompetitionCommandValidator : AbstractValidator<DeleteCompetitionCommand>
{
    public DeleteCompetitionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
