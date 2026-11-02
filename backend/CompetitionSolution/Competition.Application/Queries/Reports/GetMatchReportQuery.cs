using Competition.Application.Common.Validation;
using Competition.Application.DTOs.ImportExportDto;
using Competition.Application.DTOs.ReportDto;
using Competition.Application.Events;
using Competition.Application.Interfaces;
using Competition.Application.Lineups;
using FluentValidation;
using MediatR;
using MatchEntity = Competition.Domain.Entities.MatchEntities.Match;

namespace Competition.Application.Queries.Reports;

/// <summary>PDF report for one match (RS1 item 12, parameter: ID).</summary>
public record GetMatchReportQuery(string PublicId) : IRequest<ExportFileDto>;

public class GetMatchReportQueryValidator : AbstractValidator<GetMatchReportQuery>
{
    public GetMatchReportQueryValidator() => RuleFor(x => x.PublicId).MustBeValidPublicId();
}

public class GetMatchReportQueryHandler(
    IMatch matches, IMatchLineup lineups, IEventStore events, IStatistic statistics,
    IPublicIdLookup lookup, IPdfReportService pdf) : IRequestHandler<GetMatchReportQuery, ExportFileDto>
{
    public async Task<ExportFileDto> Handle(GetMatchReportQuery request, CancellationToken cancellationToken)
    {
        var id = await lookup.GetRequiredIdAsync<MatchEntity>(request.PublicId, cancellationToken);
        var match = await matches.GetByIdWithNavAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Match with public id '{request.PublicId}' not found.");
        var lineupMatch = await lineups.GetMatchAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Match with public id '{request.PublicId}' not found.");
        var statistic = await statistics.GetForMatchAsync(request.PublicId, cancellationToken);

        var report = new MatchReportDto
        {
            HomeName = match.HomeSelection?.Name ?? "TBD",
            AwayName = match.AwaySelection?.Name ?? "TBD",
            Date = match.Date.ToDateTimeUtc(),
            StatusName = match.Status?.Name,
            IsPostponed = match.IsPostponed,
            StadiumName = match.Stadium?.Name,
            CompetitionName = match.Round?.Season?.Competition?.Name,
            RoundName = match.Round?.Name,
            HomeScore = match.HomeScore,
            AwayScore = match.AwayScore,
            HalfHomeScore = match.HalfHomeScore,
            HalfAwayScore = match.HalfAwayScore,
            Lineup = MatchLineupMapper.ToDto(lineupMatch),
            Events = await events.GetForMatchAsync(request.PublicId, cancellationToken),
            Statistics = (statistic?.TeamStatistics ?? [])
                .OrderBy(t => t.SelectionId == match.HomeSelectionId ? 0 : 1)
                .Select(t => new TeamStatisticReportDto
                {
                    SelectionName = t.Selection.Name,
                    TotalShots = t.TotalShots,
                    ShotsOnTarget = t.ShotsOnTarget,
                    Possession = t.Possession,
                    Corners = t.Corners,
                    BigChances = t.BigChances,
                    TotalYellowCards = t.TotalYellowCards,
                    TotalRedCards = t.TotalRedCards
                })
                .ToList()
        };

        return ReportFile.Pdf(pdf.MatchReport(report), $"match-report-{match.Slug ?? match.PublicId}");
    }
}

internal static class ReportFile
{
    public static ExportFileDto Pdf(byte[] content, string baseName) => new()
    {
        Content = content,
        ContentType = "application/pdf",
        FileName = $"{baseName}.pdf"
    };
}
