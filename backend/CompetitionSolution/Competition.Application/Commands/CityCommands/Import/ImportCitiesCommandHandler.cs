using Competition.Application.Commands.CityCommands.Validators;
using Competition.Application.Common.ImportExport;
using Competition.Application.DTOs.ImportExportDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using MediatR;
using CityEntity = Competition.Domain.Entities.City;

namespace Competition.Application.Commands.CityCommands.Import;

public class ImportCitiesCommandHandler : IRequestHandler<ImportCitiesCommand, ImportResultDto>
{
    private static readonly string[] RequiredColumns = ["Name", "Country", "Population", "Latitude", "Longitude"];
    private static readonly CityImportRowValidator RowValidator = new();

    private readonly ICity _city;
    private readonly ICountry _country;
    private readonly ITabularFileService _files;
    private readonly IUnitOfWork _unitOfWork;

    public ImportCitiesCommandHandler(ICity city, ICountry country, ITabularFileService files, IUnitOfWork unitOfWork)
    {
        _city = city;
        _country = country;
        _files = files;
        _unitOfWork = unitOfWork;
    }

    public async Task<ImportResultDto> Handle(ImportCitiesCommand request, CancellationToken cancellationToken)
    {
        var (records, result) = await ImportFile.ReadAsync(_files, request.File!, RequiredColumns, cancellationToken);
        if (!result.Success) return result;

        var countriesByName = (await _country.GetAllAsync(cancellationToken))
            .Where(c => !string.IsNullOrWhiteSpace(c.Name))
            .GroupBy(c => c.Name!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var taken = (await _city.GetAllAsync(cancellationToken))
            .Select(c => c.Slug ?? string.Empty)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var toAdd = new List<CityEntity>();

        for (var i = 0; i < records.Count; i++)
        {
            var errorsBefore = result.Errors.Count;
            var reader = new ImportRowReader(records[i], ImportFile.RowNumber(i), result.Errors);
            var population = reader.Int("Population");
            var isCapital = reader.Bool("IsCapital");
            var latitude = reader.Decimal("Latitude");
            var longitude = reader.Decimal("Longitude");
            if (result.Errors.Count > errorsBefore) continue;

            var row = new CityImportRow(reader.Text("Name"), reader.Text("Country"), population!.Value, isCapital!.Value,
                latitude!.Value, longitude!.Value, reader.Text("Description"));
            var validation = RowValidator.Validate(row);
            if (!validation.IsValid) { reader.AddErrors(validation); continue; }

            if (!countriesByName.TryGetValue(row.Country, out var country))
            {
                reader.AddError("Country", $"Country '{row.Country}' does not exist. Import or create it first.");
                continue;
            }

            // Same slug = same URL, so it counts as a duplicate (existing cities and earlier rows).
            var slug = SlugHelper.GenerateSlug(row.Name);
            if (!taken.Add(slug))
            {
                reader.AddError("Name", $"A city named '{row.Name}' already exists.");
                continue;
            }

            toAdd.Add(new CityEntity
            {
                Name = row.Name,
                Slug = slug,
                Population = row.Population,
                IsCapital = row.IsCapital,
                CountryId = country.Id,
                // Attached by navigation: GeoData and the city are inserted in the same SaveChanges.
                GeoDataId = Guid.Empty,
                GeoData = new GeoData
                {
                    Latitude = row.Latitude,
                    Longitude = row.Longitude,
                    Description = string.IsNullOrWhiteSpace(row.Description) ? null : row.Description
                }
            });
        }

        // All-or-nothing: one bad row means nothing is saved.
        if (!result.Success) return result;

        foreach (var city in toAdd)
            await _city.AddAsync(city, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        result.Imported = toAdd.Count;
        return result;
    }
}
