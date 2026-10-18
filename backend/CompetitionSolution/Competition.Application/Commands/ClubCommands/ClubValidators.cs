using Competition.Application.Common.Validation;
using Competition.Application.Helpers;
using FluentValidation;
using NodaTime;
using NodaTime.Text;

namespace Competition.Application.Commands.ClubCommands;

public class SaveClubCommandValidator<T> : AbstractValidator<T> where T : SaveClubCommand
{
    public SaveClubCommandValidator(IClock clock)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100)
            .Must(x => x is not null && x.Trim().Length >= 2 && SlugHelper.GenerateSlug(x).Length > 0)
            .WithMessage("Name must contain at least two characters and a letter or number supported in a URL.");
        RuleFor(x => x.ClubAge).IsInEnum();
        RuleFor(x => x.FoundingDate)
            .Must(value => value is null || (LocalDatePattern.Iso.Parse(value).Success
                && LocalDatePattern.Iso.Parse(value).Value.Year >= 1
                && LocalDatePattern.Iso.Parse(value).Value <= clock.GetCurrentInstant().InUtc().Date))
            .WithMessage("Founding date must be a valid date (yyyy-MM-dd) that is not in the future.");
        RuleFor(x => x.CityPublicId).MustBeValidPublicId().When(x => x.CityPublicId is not null);
        RuleFor(x => x.StadiumPublicId).MustBeValidPublicId().When(x => x.StadiumPublicId is not null);
    }
}

public class CreateClubCommandValidator : SaveClubCommandValidator<CreateClubCommand>
{
    public CreateClubCommandValidator(IClock clock) : base(clock) { }
}

public class UpdateClubCommandValidator : SaveClubCommandValidator<UpdateClubCommand>
{
    public UpdateClubCommandValidator(IClock clock) : base(clock) => RuleFor(x => x.Slug).NotEmpty().MaximumLength(120);
}

public class DeleteClubCommandValidator : AbstractValidator<DeleteClubCommand>
{
    public DeleteClubCommandValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}
