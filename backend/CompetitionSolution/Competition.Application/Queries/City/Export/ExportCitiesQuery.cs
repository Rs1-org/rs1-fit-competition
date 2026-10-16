using Competition.Application.DTOs.ImportExportDto;
using MediatR;

namespace Competition.Application.Queries.City.Export;

/// <summary>Exports the city list with the same filters and sort as <c>getCities</c>, without paging.</summary>
public class ExportCitiesQuery : IRequest<ExportFileDto>
{
    public string? Format { get; set; } = "csv";
    public string? Name { get; set; }
    public string? CountryPublicId { get; set; }
    public bool? IsCapital { get; set; }
    public int? MinPopulation { get; set; }
    public int? MaxPopulation { get; set; }
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
}
