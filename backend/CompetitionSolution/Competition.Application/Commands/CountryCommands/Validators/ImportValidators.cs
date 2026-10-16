using Competition.Application.Commands.CountryCommands.Import;
using Competition.Application.Common.ImportExport;
using FluentValidation;

namespace Competition.Application.Commands.CountryCommands.Validators;

public class ImportCountriesCommandValidator : AbstractValidator<ImportCountriesCommand>
{
    public ImportCountriesCommandValidator()
    {
        RuleFor(x => x.File).MustBeImportFile();
    }
}

/// <summary>Same rules as <see cref="CreateCountryCommandValidator"/>, applied to every imported row.</summary>
public class CountryImportRowValidator : AbstractValidator<CountryImportRow>
{
    public CountryImportRowValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
