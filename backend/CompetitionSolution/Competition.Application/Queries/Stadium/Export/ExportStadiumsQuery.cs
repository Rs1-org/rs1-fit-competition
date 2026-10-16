using Competition.Application.DTOs.ImportExportDto;
using MediatR;

namespace Competition.Application.Queries.Stadium.Export;

/// <summary>Exports the stadium list with the same filters and sort as <c>getStadiums</c>, without paging.</summary>
public class ExportStadiumsQuery : IRequest<ExportFileDto>
{
    public string? Format { get; set; } = "csv";
    public string? Name { get; set; }
    public string? CountryPublicId { get; set; }
    public string? CityPublicId { get; set; }
    public int? MinCapacity { get; set; }
    public int? MaxCapacity { get; set; }
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
}
