using Competition.Application.DTOs.ImportExportDto;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Competition.Application.Commands.CityCommands.Import;

/// <summary>
/// Imports cities from a .csv/.xlsx file. Columns: Name, Country (existing country name), Population,
/// IsCapital (optional), Latitude, Longitude, Description (optional). Other columns are ignored.
/// </summary>
public class ImportCitiesCommand : IRequest<ImportResultDto>
{
    public IFormFile? File { get; set; }
}

/// <summary>One parsed row; property names are the column names so validation errors point at the right column.</summary>
public record CityImportRow(string Name, string Country, int Population, bool IsCapital, decimal Latitude, decimal Longitude, string Description);
