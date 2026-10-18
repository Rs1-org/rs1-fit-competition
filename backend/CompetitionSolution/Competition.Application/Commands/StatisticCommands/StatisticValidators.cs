using Competition.Application.Common.Validation;
using Competition.Application.DTOs.StatisticDto;
using FluentValidation;

namespace Competition.Application.Commands.StatisticCommands;

public class TeamStatisticValuesValidator : AbstractValidator<TeamStatisticValues>
{
    public TeamStatisticValuesValidator()
    {
        RuleFor(x => x.TotalShots).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ShotsOnTarget).GreaterThanOrEqualTo(0).LessThanOrEqualTo(x => x.TotalShots);
        RuleFor(x => x.TotalRedCards).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TotalYellowCards).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Corners).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BigChances).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Possession).InclusiveBetween(0, 100);
    }
}

public class SaveStatisticCommandValidator<T> : AbstractValidator<T> where T : SaveStatisticCommand
{
    public SaveStatisticCommandValidator()
    {
        RuleFor(x => x.Home).NotNull().SetValidator(new TeamStatisticValuesValidator()!);
        RuleFor(x => x.Away).NotNull().SetValidator(new TeamStatisticValuesValidator()!);
        RuleFor(x => x).Must(x => x.Home is null || x.Away is null || (long)x.Home.Possession + x.Away.Possession == 100)
            .WithMessage("Home and away possession must sum to 100.").OverridePropertyName("Possession");
    }
}

public class CreateStatisticCommandValidator : SaveStatisticCommandValidator<CreateStatisticCommand>
{
    public CreateStatisticCommandValidator() => RuleFor(x => x.MatchPublicId).MustBeValidPublicId();
}

public class UpdateStatisticCommandValidator : SaveStatisticCommandValidator<UpdateStatisticCommand>
{
    public UpdateStatisticCommandValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}

public class DeleteStatisticCommandValidator : AbstractValidator<DeleteStatisticCommand>
{
    public DeleteStatisticCommandValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}
