using Competition.Application.DTOs.ImportExportDto;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Competition.Application.Commands.StadiumCommands.Import;

/// <summary>
/// Imports stadiums from a .csv/.xlsx file. Columns: Name, City and Country (existing names), Capacity,
/// DateOfConstruction (yyyy-MM-dd), Latitude, Longitude, GeoDescription. Other columns are ignored.
/// </summary>
public class ImportStadiumsCommand : IRequest<ImportResultDto>
{
    public IFormFile? File { get; set; }
}

/// <summary>One parsed row; property names are the column names so validation errors point at the right column.</summary>
public record StadiumImportRow(string Name, string City, string Country, int Capacity, DateTime DateOfConstruction,
    decimal Latitude, decimal Longitude, string GeoDescription);
