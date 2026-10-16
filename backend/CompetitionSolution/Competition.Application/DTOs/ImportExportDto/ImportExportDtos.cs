namespace Competition.Application.DTOs.ImportExportDto;

/// <summary>A generated export file; the controller returns it as a download.</summary>
public class ExportFileDto
{
    public byte[] Content { get; set; } = [];
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

/// <summary>One problem in an imported file. <see cref="Row"/> is the spreadsheet row number (header = row 1).</summary>
public class ImportErrorDto
{
    public int Row { get; set; }
    public string Column { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>Import outcome. Imports are all-or-nothing: when <see cref="Errors"/> is not empty, nothing was saved.</summary>
public class ImportResultDto
{
    public int Imported { get; set; }
    public int TotalRows { get; set; }
    public List<ImportErrorDto> Errors { get; set; } = [];
    public bool Success => Errors.Count == 0;
}
