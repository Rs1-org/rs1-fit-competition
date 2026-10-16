using Competition.Application.Commands.StadiumCommands.Import;
using Competition.Application.Common.ImportExport;
using FluentValidation;

namespace Competition.Application.Commands.StadiumCommands.Validators;

public class ImportStadiumsCommandValidator : AbstractValidator<ImportStadiumsCommand>
{
    public ImportStadiumsCommandValidator()
    {
        RuleFor(x => x.File).MustBeImportFile();
    }
}

/// <summary>Same rules as <see cref="CreateStadiumCommandValidator"/>, applied to every imported row.</summary>
public class StadiumImportRowValidator : AbstractValidator<StadiumImportRow>
{
    public StadiumImportRowValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.Country).NotEmpty();
        RuleFor(x => x.Capacity).GreaterThan(0);
        RuleFor(x => x.DateOfConstruction).LessThanOrEqualTo(_ => DateTime.UtcNow)
            .WithMessage("'DateOfConstruction' cannot be in the future.");
        RuleFor(x => x.GeoDescription).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
    }
}
