using Competition.Application.DTOs.ReportDto;

namespace Competition.Application.Interfaces;

/// <summary>Renders the PDF reports (RS1 item 12). Handlers gather the data, this only lays it out.</summary>
public interface IPdfReportService
{
    byte[] MatchReport(MatchReportDto report);
    byte[] MatchesReport(MatchesReportDto report);
}
