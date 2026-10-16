using Competition.Application.Common.ImportExport;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.ImportExportDto;
using Competition.Application.Interfaces;
using MediatR;
using CountryEntity = Competition.Domain.Entities.Country;

namespace Competition.Application.Queries.City.Export;

public class ExportCitiesQueryHandler : IRequestHandler<ExportCitiesQuery, ExportFileDto>
{
    /// <summary>Same columns the import reads, so an exported file can be edited and imported back.</summary>
    public static readonly string[] Columns = ["PublicId", "Name", "Country", "Population", "IsCapital", "Latitude", "Longitude", "Description"];

    private readonly ICity _city;
    private readonly IPublicIdLookup _lookup;
    private readonly ITabularFileService _files;

    public ExportCitiesQueryHandler(ICity city, IPublicIdLookup lookup, ITabularFileService files)
    {
        _city = city;
        _lookup = lookup;
        _files = files;
    }

    public async Task<ExportFileDto> Handle(ExportCitiesQuery request, CancellationToken cancellationToken)
    {
        var format = ImportExportFormat.Parse(request.Format);

        // Same rule as the list: an unknown country filter matches nothing (empty file, not a 404).
        var countryId = await _lookup.GetIdByPublicIdAsync<CountryEntity>(request.CountryPublicId, cancellationToken);
        var cities = request.CountryPublicId is not null && countryId is null
            ? []
            : await _city.GetFilteredListAsync(
                new CityListFilter(request.Name, countryId, request.IsCapital, request.MinPopulation, request.MaxPopulation,
                    new SortSpec(request.SortBy, request.SortDir)),
                ImportExportFormat.MaxExportRows, cancellationToken);

        var rows = cities.Select(c => (IReadOnlyList<object?>)
        [
            c.PublicId, c.Name, c.Country?.Name, c.Population, c.IsCapital,
            c.GeoData?.Latitude, c.GeoData?.Longitude, c.GeoData?.Description
        ]);

        return ImportExportFormat.ToFile(_files.Write(Columns, rows, format, "Cities"), format, "cities");
    }
}
