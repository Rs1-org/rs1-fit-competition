using Competition.Application.DTOs.ImportExportDto;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Competition.Application.Commands.CountryCommands.Import;

/// <summary>Imports countries from a .csv/.xlsx file with a "Name" column (other columns are ignored).</summary>
public class ImportCountriesCommand : IRequest<ImportResultDto>
{
    public IFormFile? File { get; set; }
}

/// <summary>One parsed row; property names are the column names so validation errors point at the right column.</summary>
public record CountryImportRow(string Name);
