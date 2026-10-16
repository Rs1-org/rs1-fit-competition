namespace Competition.Application.Interfaces;

public enum TabularFormat
{
    Csv,
    Xlsx,
    Json
}

/// <summary>
/// Reads and writes simple header + rows files (CSV, Excel and JSON) for import/export (RS1 item 86).
/// Values are plain .NET values on write (string, int, decimal, bool, DateTime, null)
/// and invariant-culture strings on read, so the handlers stay format-agnostic.
/// </summary>
public interface ITabularFileService
{
    byte[] Write(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<object?>> rows, TabularFormat format, string sheetName);

    /// <summary>
    /// Returns one dictionary per data row (header → trimmed value, header lookup is case-insensitive).
    /// Fully empty rows are skipped. Throws <see cref="InvalidDataException"/> for an unreadable file.
    /// </summary>
    Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> ReadAsync(Stream content, TabularFormat format, CancellationToken cancellationToken = default);
}
