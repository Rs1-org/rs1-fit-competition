using Competition.Application.DTOs.ImportExportDto;
using MediatR;

namespace Competition.Application.Queries.Country.Export;

/// <summary>Exports the country list with the same filters and sort as <c>getAll</c>, without paging.</summary>
public class ExportCountriesQuery : IRequest<ExportFileDto>
{
    public string? Format { get; set; } = "csv";
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public bool? HasPicture { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
}
