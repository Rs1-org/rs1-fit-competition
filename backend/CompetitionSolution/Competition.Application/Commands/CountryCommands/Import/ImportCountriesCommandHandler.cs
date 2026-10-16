using Competition.Application.Commands.CountryCommands.Validators;
using Competition.Application.Common.ImportExport;
using Competition.Application.DTOs.ImportExportDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using MediatR;
using CountryEntity = Competition.Domain.Entities.Country;

namespace Competition.Application.Commands.CountryCommands.Import;

public class ImportCountriesCommandHandler : IRequestHandler<ImportCountriesCommand, ImportResultDto>
{
    private static readonly string[] RequiredColumns = ["Name"];
    private static readonly CountryImportRowValidator RowValidator = new();

    private readonly ICountry _country;
    private readonly ITabularFileService _files;
    private readonly IUnitOfWork _unitOfWork;

    public ImportCountriesCommandHandler(ICountry country, ITabularFileService files, IUnitOfWork unitOfWork)
    {
        _country = country;
        _files = files;
        _unitOfWork = unitOfWork;
    }

    public async Task<ImportResultDto> Handle(ImportCountriesCommand request, CancellationToken cancellationToken)
    {
        var (records, result) = await ImportFile.ReadAsync(_files, request.File!, RequiredColumns, cancellationToken);
        if (!result.Success) return result;

        var taken = (await _country.GetAllAsync(cancellationToken))
            .Select(c => c.Slug ?? string.Empty)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var toAdd = new List<CountryEntity>();

        for (var i = 0; i < records.Count; i++)
        {
            var reader = new ImportRowReader(records[i], ImportFile.RowNumber(i), result.Errors);
            var row = new CountryImportRow(reader.Text("Name"));

            var validation = RowValidator.Validate(row);
            if (!validation.IsValid) { reader.AddErrors(validation); continue; }

            // Same slug = same URL, so it counts as a duplicate (existing countries and earlier rows).
            var slug = SlugHelper.GenerateSlug(row.Name);
            if (!taken.Add(slug))
            {
                reader.AddError("Name", $"A country named '{row.Name}' already exists.");
                continue;
            }

            toAdd.Add(new CountryEntity { Name = row.Name, Slug = slug });
        }

        // All-or-nothing: one bad row means nothing is saved.
        if (!result.Success) return result;

        foreach (var country in toAdd)
            await _country.AddAsync(country, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        result.Imported = toAdd.Count;
        return result;
    }
}
