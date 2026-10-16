using Competition.Application.Common.ImportExport;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.ImportExportDto;
using Competition.Application.Interfaces;
using MediatR;
using CityEntity = Competition.Domain.Entities.City;
using CountryEntity = Competition.Domain.Entities.Country;

namespace Competition.Application.Queries.Stadium.Export;

public class ExportStadiumsQueryHandler : IRequestHandler<ExportStadiumsQuery, ExportFileDto>
{
    /// <summary>Same columns the import reads, so an exported file can be edited and imported back.</summary>
    public static readonly string[] Columns =
        ["PublicId", "Name", "City", "Country", "Capacity", "DateOfConstruction", "Latitude", "Longitude", "GeoDescription"];

    private readonly IStadium _stadium;
    private readonly IPublicIdLookup _lookup;
    private readonly ITabularFileService _files;

    public ExportStadiumsQueryHandler(IStadium stadium, IPublicIdLookup lookup, ITabularFileService files)
    {
        _stadium = stadium;
        _lookup = lookup;
        _files = files;
    }

    public async Task<ExportFileDto> Handle(ExportStadiumsQuery request, CancellationToken cancellationToken)
    {
        var format = ImportExportFormat.Parse(request.Format);

        // Same rule as the list: an unknown filter matches nothing (empty file, not a 404).
        var countryId = await _lookup.GetIdByPublicIdAsync<CountryEntity>(request.CountryPublicId, cancellationToken);
        var cityId = await _lookup.GetIdByPublicIdAsync<CityEntity>(request.CityPublicId, cancellationToken);
        var unknownFilter = (request.CountryPublicId is not null && countryId is null)
                            || (request.CityPublicId is not null && cityId is null);

        var stadiums = unknownFilter
            ? []
            : await _stadium.GetFilteredListAsync(
                new StadiumListFilter(request.Name, countryId, cityId, request.MinCapacity, request.MaxCapacity,
                    new SortSpec(request.SortBy, request.SortDir)),
                ImportExportFormat.MaxExportRows, cancellationToken);

        var rows = stadiums.Select(s => (IReadOnlyList<object?>)
        [
            s.PublicId, s.Name, s.City?.Name, s.City?.Country?.Name, s.Capacity, s.DateOfConstruction.Date,
            s.GeoData?.Latitude, s.GeoData?.Longitude, s.GeoData?.Description
        ]);

        return ImportExportFormat.ToFile(_files.Write(Columns, rows, format, "Stadiums"), format, "stadiums");
    }
}
