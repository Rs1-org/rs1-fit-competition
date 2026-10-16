using Competition.Application.DTOs.ImportExportDto;
using Competition.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Competition.Application.Common.ImportExport;

public static class ImportFile
{
    /// <summary>
    /// Reads the uploaded file into records. File-level problems (unreadable file, missing columns,
    /// no data, too many rows) are returned in <c>result.Errors</c> with Row 1 (the header row) instead of throwing.
    /// </summary>
    public static async Task<(IReadOnlyList<IReadOnlyDictionary<string, string>> Records, ImportResultDto Result)> ReadAsync(
        ITabularFileService files, IFormFile file, IReadOnlyList<string> requiredColumns, CancellationToken cancellationToken)
    {
        var result = new ImportResultDto();
        IReadOnlyList<IReadOnlyDictionary<string, string>> records;

        try
        {
            await using var stream = file.OpenReadStream();
            records = await files.ReadAsync(stream, ImportExportFormat.FromFileName(file.FileName), cancellationToken);
        }
        catch (InvalidDataException ex)
        {
            result.Errors.Add(new ImportErrorDto { Row = 1, Message = ex.Message });
            return ([], result);
        }

        result.TotalRows = records.Count;
        if (records.Count == 0)
            result.Errors.Add(new ImportErrorDto { Row = 2, Message = "The file has no data rows." });
        else if (records.Count > ImportExportFormat.MaxImportRows)
            result.Errors.Add(new ImportErrorDto { Row = 1, Message = $"At most {ImportExportFormat.MaxImportRows} rows can be imported at once." });

        foreach (var column in ImportRowReader.MissingColumns(records, requiredColumns))
            result.Errors.Add(new ImportErrorDto { Row = 1, Column = column, Message = $"Missing column '{column}'." });

        return (records, result);
    }

    /// <summary>Spreadsheet row number of the record at <paramref name="index"/> (header is row 1).</summary>
    public static int RowNumber(int index) => index + 2;
}
