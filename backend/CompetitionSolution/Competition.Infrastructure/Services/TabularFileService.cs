using System.Globalization;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using Competition.Application.Interfaces;

namespace Competition.Infrastructure.Services;

public class TabularFileService : ITabularFileService
{
    private const string DateFormat = "yyyy-MM-dd";
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public byte[] Write(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<object?>> rows, TabularFormat format, string sheetName) => format switch
    {
        TabularFormat.Xlsx => WriteXlsx(headers, rows, sheetName),
        TabularFormat.Json => WriteJson(headers, rows),
        _ => WriteCsv(headers, rows)
    };

    public async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> ReadAsync(Stream content, TabularFormat format, CancellationToken cancellationToken = default)
    {
        // Buffer first: ClosedXML needs a seekable stream and IFormFile streams are not always seekable.
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        if (format == TabularFormat.Json)
            return ReadJson(buffer);

        var table = format == TabularFormat.Xlsx ? ReadXlsx(buffer) : await ReadCsvAsync(buffer, cancellationToken);
        return ToRecords(table);
    }

    // ---------- CSV (RFC 4180) ----------

    private static byte[] WriteCsv(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<object?>> rows)
    {
        var sb = new StringBuilder();
        sb.AppendJoin(',', headers.Select(Escape)).Append("\r\n");
        foreach (var row in rows)
            sb.AppendJoin(',', row.Select(v => Escape(v is string text ? NeutralizeFormula(text) : Format(v)))).Append("\r\n");

        // UTF-8 with BOM so Excel shows č, ć, š, ž, đ correctly when the CSV is opened directly.
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    // Excel runs a CSV cell starting with = + - @ as a formula; prefix text values so they stay text.
    private static string NeutralizeFormula(string value) =>
        value.Length > 0 && "=+-@".Contains(value[0]) ? "'" + value : value;

    private static string Escape(string value) =>
        value.IndexOfAny([',', '"', '\r', '\n']) >= 0 ? $"\"{value.Replace("\"", "\"\"")}\"" : value;

    private static async Task<List<List<string>>> ReadCsvAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var text = await reader.ReadToEndAsync(cancellationToken);

        // Excel in some locales saves CSV with ';', so pick the delimiter from the header line.
        var headerLine = text.Split('\n', 2)[0];
        var delimiter = headerLine.Count(c => c == ';') > headerLine.Count(c => c == ',') ? ';' : ',';

        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (inQuotes)
            {
                if (c == '"' && i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                else if (c == '"') inQuotes = false;
                else field.Append(c);
            }
            else if (c == '"') inQuotes = true;
            else if (c == delimiter) { row.Add(field.ToString()); field.Clear(); }
            else if (c == '\n')
            {
                row.Add(field.ToString().TrimEnd('\r'));
                field.Clear();
                rows.Add(row);
                row = new List<string>();
            }
            else field.Append(c);
        }

        if (inQuotes)
            throw new InvalidDataException("The CSV file has an unclosed quoted value.");
        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString().TrimEnd('\r'));
            rows.Add(row);
        }
        return rows;
    }

    // ---------- Excel ----------

    private static byte[] WriteXlsx(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<object?>> rows, string sheetName)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(sheetName);

        for (var c = 0; c < headers.Count; c++)
            sheet.Cell(1, c + 1).Value = headers[c];
        sheet.Row(1).Style.Font.Bold = true;

        var r = 2;
        foreach (var row in rows)
        {
            for (var c = 0; c < row.Count; c++)
            {
                var cell = sheet.Cell(r, c + 1);
                switch (row[c])
                {
                    case null: break;
                    case int i: cell.Value = i; break;
                    case decimal d: cell.Value = d; break;
                    case double db: cell.Value = db; break;
                    case bool b: cell.Value = b; break;
                    case DateTime dt:
                        cell.Value = dt;
                        cell.Style.DateFormat.Format = DateFormat;
                        break;
                    default: cell.Value = Format(row[c]); break;
                }
            }
            r++;
        }

        sheet.SheetView.FreezeRows(1);
        sheet.Columns().AdjustToContents();

        using var output = new MemoryStream();
        workbook.SaveAs(output);
        return output.ToArray();
    }

    private static List<List<string>> ReadXlsx(Stream stream)
    {
        // Any ClosedXML failure (not a workbook, corrupt parts, odd cells) is reported as a bad file, not a 500.
        try
        {
            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheets.FirstOrDefault()
                ?? throw new InvalidDataException("The Excel file has no worksheets.");
            var used = sheet.RangeUsed();
            if (used is null) return [];

            var lastColumn = used.LastColumn().ColumnNumber();
            return used.Rows()
                .Select(row => Enumerable.Range(1, lastColumn)
                    .Select(col => CellText(sheet.Cell(row.RowNumber(), col)))
                    .ToList())
                .ToList();
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not InvalidDataException)
        {
            throw new InvalidDataException("The file is not a valid Excel (.xlsx) workbook.", ex);
        }
    }

    private static string CellText(IXLCell cell)
    {
        var value = cell.Value;
        if (value.IsBlank) return string.Empty;
        if (value.IsDateTime) return value.GetDateTime().ToString(DateFormat, Invariant);
        if (value.IsNumber) return value.GetNumber().ToString(Invariant);
        if (value.IsBoolean) return value.GetBoolean() ? "true" : "false";
        return cell.GetString();
    }

    // ---------- JSON ----------

    private static byte[] WriteJson(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<object?>> rows)
    {
        var objects = rows.Select(row =>
        {
            var obj = new Dictionary<string, object?>();
            for (var c = 0; c < headers.Count; c++)
            {
                var value = c < row.Count ? row[c] : null;
                obj[headers[c]] = value is DateTime dt ? dt.ToString(DateFormat, Invariant) : value;
            }
            return obj;
        });
        return JsonSerializer.SerializeToUtf8Bytes(objects, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>Expects an array of flat objects (property name = column), as written by the export.</summary>
    private static List<IReadOnlyDictionary<string, string>> ReadJson(Stream stream)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(stream);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("The file is not valid JSON.", ex);
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Array)
                throw new InvalidDataException("The JSON file must contain an array of objects.");

            var records = new List<IReadOnlyDictionary<string, string>>();
            foreach (var item in document.RootElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                    throw new InvalidDataException("Every item of the JSON array must be an object.");

                var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var property in item.EnumerateObject())
                {
                    record.TryAdd(property.Name.Trim(), property.Value.ValueKind switch
                    {
                        JsonValueKind.String => property.Value.GetString()!.Trim(),
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
                        _ => property.Value.GetRawText()
                    });
                }
                if (record.Values.Any(v => v.Length > 0)) records.Add(record);
            }
            return records;
        }
    }

    // ---------- shared ----------

    private static string Format(object? value) => value switch
    {
        null => string.Empty,
        DateTime dt => dt.ToString(DateFormat, Invariant),
        bool b => b ? "true" : "false",
        IFormattable f => f.ToString(null, Invariant),
        _ => value.ToString() ?? string.Empty
    };

    private static List<IReadOnlyDictionary<string, string>> ToRecords(List<List<string>> table)
    {
        if (table.Count == 0)
            throw new InvalidDataException("The file is empty; the first row must contain the column headers.");

        var headers = table[0].Select(h => h.Trim()).ToList();
        var records = new List<IReadOnlyDictionary<string, string>>();

        foreach (var row in table.Skip(1))
        {
            if (row.All(string.IsNullOrWhiteSpace)) continue;

            var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var c = 0; c < headers.Count; c++)
            {
                if (headers[c].Length == 0 || record.ContainsKey(headers[c])) continue;
                record[headers[c]] = c < row.Count ? row[c].Trim() : string.Empty;
            }
            records.Add(record);
        }
        return records;
    }
}
