using Competition.Application.DTOs.ImportExportDto;
using MediatR;

namespace Competition.Application.Queries.Match.Export;

/// <summary>Exports the match list with the same filters and sort as <c>getAll</c>, without paging.</summary>
public class ExportMatchesQuery : IRequest<ExportFileDto>
{
    public string? Format { get; set; } = "csv";
    public string? StatusPublicId { get; set; }
    public string? StadiumPublicId { get; set; }
    public string? CompetitionPublicId { get; set; }
    public bool? IsPostponed { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Phase { get; set; }
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
}
