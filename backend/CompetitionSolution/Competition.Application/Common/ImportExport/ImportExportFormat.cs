using Competition.Application.DTOs.ImportExportDto;
using Competition.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Competition.Application.Common.ImportExport;

/// <summary>Formats, limits and file rules shared by the import/export handlers (RS1 item 86).</summary>
public static class ImportExportFormat
{
    public const int MaxExportRows = 10_000;
    public const int MaxImportRows = 1_000;
    public const long MaxImportBytes = 2 * 1024 * 1024;

    public static readonly string[] Formats = ["csv", "xlsx", "json"];
    private static readonly string[] Extensions = [".csv", ".xlsx", ".json"];

    public static TabularFormat Parse(string? format) => format?.ToLowerInvariant() switch
    {
        "xlsx" => TabularFormat.Xlsx,
        "json" => TabularFormat.Json,
        _ => TabularFormat.Csv
    };

    public static TabularFormat FromFileName(string fileName) => Parse(Path.GetExtension(fileName).TrimStart('.'));

    public static ExportFileDto ToFile(byte[] content, TabularFormat format, string baseName) => new()
    {
        Content = content,
        ContentType = format switch
        {
            TabularFormat.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            TabularFormat.Json => "application/json; charset=utf-8",
            _ => "text/csv; charset=utf-8"
        },
        FileName = $"{baseName}-{DateTime.UtcNow:yyyyMMdd-HHmm}.{format.ToString().ToLowerInvariant()}"
    };

    public static IRuleBuilderOptions<T, string?> MustBeExportFormat<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(f => f is not null && Formats.Contains(f, StringComparer.OrdinalIgnoreCase))
            .WithMessage("'{PropertyName}' must be 'csv', 'xlsx' or 'json'.");

    /// <summary>Required, .csv, .xlsx or .json, not empty and at most 2 MB.</summary>
    public static IRuleBuilderOptions<T, IFormFile?> MustBeImportFile<T>(this IRuleBuilder<T, IFormFile?> rule) =>
        rule.NotNull().WithMessage("Choose a .csv, .xlsx or .json file to import.")
            .Must(f => f is null || Extensions.Contains(Path.GetExtension(f.FileName), StringComparer.OrdinalIgnoreCase))
            .WithMessage("Only .csv, .xlsx and .json files can be imported.")
            .Must(f => f is null || f.Length > 0).WithMessage("The file is empty.")
            .Must(f => f is null || f.Length <= MaxImportBytes).WithMessage("The file can be at most 2 MB.");
}
