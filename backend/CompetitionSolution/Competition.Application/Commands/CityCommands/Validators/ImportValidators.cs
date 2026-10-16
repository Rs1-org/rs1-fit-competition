using Competition.Application.Commands.CityCommands.Import;
using Competition.Application.Common.ImportExport;
using FluentValidation;

namespace Competition.Application.Commands.CityCommands.Validators;

public class ImportCitiesCommandValidator : AbstractValidator<ImportCitiesCommand>
{
    public ImportCitiesCommandValidator()
    {
        RuleFor(x => x.File).MustBeImportFile();
    }
}

/// <summary>Same rules as <see cref="CreateCityCommandValidator"/>, applied to every imported row.</summary>
public class CityImportRowValidator : AbstractValidator<CityImportRow>
{
    public CityImportRowValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Country).NotEmpty();
        RuleFor(x => x.Population).GreaterThan(0);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
