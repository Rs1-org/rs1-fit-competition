using Competition.Application.Commands.StadiumCommands.Validators;
using Competition.Application.Common.ImportExport;
using Competition.Application.DTOs.ImportExportDto;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using MediatR;
using StadiumEntity = Competition.Domain.Entities.MatchEntities.Stadium;

namespace Competition.Application.Commands.StadiumCommands.Import;

public class ImportStadiumsCommandHandler : IRequestHandler<ImportStadiumsCommand, ImportResultDto>
{
    private static readonly string[] RequiredColumns =
        ["Name", "City", "Country", "Capacity", "DateOfConstruction", "Latitude", "Longitude", "GeoDescription"];
    private static readonly StadiumImportRowValidator RowValidator = new();

    private readonly IStadium _stadium;
    private readonly ICity _city;
    private readonly ITabularFileService _files;
    private readonly IUnitOfWork _unitOfWork;

    public ImportStadiumsCommandHandler(IStadium stadium, ICity city, ITabularFileService files, IUnitOfWork unitOfWork)
    {
        _stadium = stadium;
        _city = city;
        _files = files;
        _unitOfWork = unitOfWork;
    }

    public async Task<ImportResultDto> Handle(ImportStadiumsCommand request, CancellationToken cancellationToken)
    {
        var (records, result) = await ImportFile.ReadAsync(_files, request.File!, RequiredColumns, cancellationToken);
        if (!result.Success) return result;

        // City names repeat across countries, so a city is identified by "city|country".
        var citiesByKey = (await _city.GetAllAsync(cancellationToken))
            .Where(c => !string.IsNullOrWhiteSpace(c.Name) && c.Country?.Name is not null)
            .GroupBy(c => CityKey(c.Name!, c.Country!.Name!), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var taken = (await _stadium.GetAllAsync(cancellationToken))
            .Select(s => s.Slug ?? string.Empty)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var toAdd = new List<StadiumEntity>();

        for (var i = 0; i < records.Count; i++)
        {
            var errorsBefore = result.Errors.Count;
            var reader = new ImportRowReader(records[i], ImportFile.RowNumber(i), result.Errors);
            var capacity = reader.Int("Capacity");
            var built = reader.Date("DateOfConstruction");
            var latitude = reader.Decimal("Latitude");
            var longitude = reader.Decimal("Longitude");
            if (result.Errors.Count > errorsBefore) continue;

            var row = new StadiumImportRow(reader.Text("Name"), reader.Text("City"), reader.Text("Country"), capacity!.Value,
                built!.Value, latitude!.Value, longitude!.Value, reader.Text("GeoDescription"));
            var validation = RowValidator.Validate(row);
            if (!validation.IsValid) { reader.AddErrors(validation); continue; }

            if (!citiesByKey.TryGetValue(CityKey(row.City, row.Country), out var city))
            {
                reader.AddError("City", $"City '{row.City}' in '{row.Country}' does not exist. Import or create it first.");
                continue;
            }

            // Same slug = same URL, so it counts as a duplicate (existing stadiums and earlier rows).
            var slug = SlugHelper.GenerateSlug(row.Name);
            if (!taken.Add(slug))
            {
                reader.AddError("Name", $"A stadium named '{row.Name}' already exists.");
                continue;
            }

            toAdd.Add(new StadiumEntity
            {
                Name = row.Name,
                Slug = slug,
                Capacity = row.Capacity,
                DateOfConstruction = row.DateOfConstruction,
                CityId = city.Id,
                // Attached by navigation: GeoData and the stadium are inserted in the same SaveChanges.
                GeoData = new GeoData { Latitude = row.Latitude, Longitude = row.Longitude, Description = row.GeoDescription }
            });
        }

        // All-or-nothing: one bad row means nothing is saved.
        if (!result.Success) return result;

        foreach (var stadium in toAdd)
            await _stadium.AddAsync(stadium, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        result.Imported = toAdd.Count;
        return result;
    }

    private static string CityKey(string city, string country) => $"{city.Trim()}|{country.Trim()}";
}
