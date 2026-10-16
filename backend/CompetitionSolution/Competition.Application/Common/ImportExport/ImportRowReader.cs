using System.Globalization;
using Competition.Application.DTOs.ImportExportDto;
using FluentValidation.Results;

namespace Competition.Application.Common.ImportExport;

/// <summary>
/// Reads typed values from one imported row and collects parse errors with their row and column,
/// so a whole file can be checked before anything is saved.
/// </summary>
public class ImportRowReader
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
    private static readonly string[] TrueValues = ["true", "yes", "1", "da"];
    private static readonly string[] FalseValues = ["false", "no", "0", "ne", ""];

    private readonly IReadOnlyDictionary<string, string> _values;
    private readonly List<ImportErrorDto> _errors;

    public ImportRowReader(IReadOnlyDictionary<string, string> values, int row, List<ImportErrorDto> errors)
    {
        _values = values;
        Row = row;
        _errors = errors;
    }

    public int Row { get; }

    public string Text(string column) => _values.TryGetValue(column, out var v) ? v : string.Empty;

    public int? Int(string column)
    {
        var text = Text(column);
        if (int.TryParse(text, NumberStyles.Integer, Invariant, out var value)) return value;
        // Excel stores whole numbers as doubles ("105000" may arrive as "105000.0").
        if (decimal.TryParse(text, NumberStyles.Number, Invariant, out var d) && d == decimal.Truncate(d) && d is >= int.MinValue and <= int.MaxValue)
            return (int)d;
        AddError(column, text.Length == 0 ? $"'{column}' is required." : $"'{text}' is not a whole number.");
        return null;
    }

    public decimal? Decimal(string column)
    {
        var text = Text(column).Replace(',', '.');
        if (decimal.TryParse(text, NumberStyles.Number, Invariant, out var value)) return value;
        AddError(column, text.Length == 0 ? $"'{column}' is required." : $"'{text}' is not a number.");
        return null;
    }

    public bool? Bool(string column)
    {
        var text = Text(column).ToLowerInvariant();
        if (TrueValues.Contains(text)) return true;
        if (FalseValues.Contains(text)) return false;
        AddError(column, $"'{Text(column)}' is not true/false.");
        return null;
    }

    public DateTime? Date(string column)
    {
        var text = Text(column);
        if (DateTime.TryParseExact(text, ["yyyy-MM-dd", "dd.MM.yyyy", "d.M.yyyy"], Invariant, DateTimeStyles.None, out var value))
            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        AddError(column, text.Length == 0 ? $"'{column}' is required." : $"'{text}' is not a date (use yyyy-MM-dd).");
        return null;
    }

    public void AddError(string column, string message) =>
        _errors.Add(new ImportErrorDto { Row = Row, Column = column, Message = message });

    /// <summary>Adds the FluentValidation failures of a typed row; property names are the column names.</summary>
    public void AddErrors(ValidationResult result)
    {
        foreach (var failure in result.Errors)
            AddError(failure.PropertyName, failure.ErrorMessage);
    }

    /// <summary>Returns the column names missing from the file's header row.</summary>
    public static IReadOnlyList<string> MissingColumns(IReadOnlyList<IReadOnlyDictionary<string, string>> records, IEnumerable<string> required) =>
        records.Count == 0 ? [] : required.Where(c => !records[0].ContainsKey(c)).ToList();
}
