using System.Globalization;
using Competition.Application.DTOs.MatchDto;
using Competition.Application.DTOs.ReportDto;
using Competition.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Competition.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Competition.Infrastructure.Services;

public class PdfReportService : IPdfReportService
{
    private const string NotRecorded = "Not recorded";
    private readonly DateTimeZone _zone;

    // QuestPDF refuses to generate anything until a license type is set.
    static PdfReportService() => QuestPDF.Settings.License = LicenseType.Community;

    public PdfReportService(IOptions<ReportSettings> settings)
    {
        // Match times are stored in UTC; the report shows them in the configured local zone, like the app does.
        _zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(settings.Value.TimeZone ?? string.Empty) ?? DateTimeZone.Utc;
    }

    public byte[] MatchReport(MatchReportDto report) =>
        Render("Match report", $"{report.HomeName} vs {report.AwayName}", column =>
        {
            column.Item().Element(c => Facts(c, MatchFacts(report)));

            Section(column, "Lineups", c => c.Row(row =>
            {
                row.Spacing(16);
                row.RelativeItem().Element(side => Lineup(side, report.HomeName, report.Lineup.HomeFormationCode, report.Lineup.Home));
                row.RelativeItem().Element(side => Lineup(side, report.AwayName, report.Lineup.AwayFormationCode, report.Lineup.Away));
            }));

            Section(column, "Event timeline", c =>
            {
                if (report.Events.Count == 0)
                {
                    c.Text(NotRecorded);
                    return;
                }
                Table(c, [("Min", 1), ("Part", 2), ("Event", 2), ("Team", 3), ("Player", 4)],
                    report.Events.Select(e => new[]
                    {
                        e.AddedMinute > 0 ? $"{e.Minute}+{e.AddedMinute}'" : $"{e.Minute}'",
                        e.MatchPart.Name,
                        e.EventType.Name,
                        e.Selection.Name,
                        e.RelatedPlayer is null ? e.Player?.Name : $"{e.Player?.Name} / {e.RelatedPlayer.Name}"
                    }));
            });

            Section(column, "Statistics", c =>
            {
                if (report.Statistics.Count == 0)
                {
                    c.Text(NotRecorded);
                    return;
                }
                Table(c, [("Team", 4), ("Shots", 1), ("On target", 2), ("Possession", 2), ("Corners", 2), ("Big chances", 2), ("Yellow", 1), ("Red", 1)],
                    report.Statistics.Select(s => new[]
                    {
                        s.SelectionName, Number(s.TotalShots), Number(s.ShotsOnTarget), $"{Number(s.Possession)}%",
                        Number(s.Corners), Number(s.BigChances), Number(s.TotalYellowCards), Number(s.TotalRedCards)
                    }));
            });
        });

    public byte[] MatchesReport(MatchesReportDto report) =>
        Render("Matches report", $"{DayText(report.DateFrom)} - {DayText(report.DateTo)}", column =>
        {
            if (report.Filters.Count > 0)
                column.Item().Text(string.Join(" · ", report.Filters));

            column.Item().Text(report.Rows.Count < report.TotalCount
                ? $"Showing the first {report.Rows.Count} of {report.TotalCount} matches."
                : $"{report.TotalCount} matches.");

            if (report.Rows.Count == 0)
                return;

            column.Item().Element(c => Table(c,
                [("Date", 4), ("Match", 6), ("Result", 2), ("Status", 3), ("Stadium", 3), ("Competition", 3)],
                report.Rows.Select(r => new[]
                {
                    DateText(r.Date), $"{r.HomeName} vs {r.AwayName}", Score(r.HomeScore, r.AwayScore),
                    r.IsPostponed ? $"{r.StatusName} (postponed)" : r.StatusName, r.StadiumName, r.CompetitionName
                })));
        });

    private IEnumerable<(string Label, string? Value)> MatchFacts(MatchReportDto report)
    {
        yield return ("Date", DateText(report.Date));
        yield return ("Stadium", report.StadiumName);
        yield return ("Status", report.IsPostponed ? $"{report.StatusName} (postponed)" : report.StatusName);
        yield return ("Competition", report.CompetitionName);
        yield return ("Round", report.RoundName);
        yield return ("Referee", report.Lineup.RefereeName);
        yield return ("Final score", Score(report.HomeScore, report.AwayScore));
        yield return ("Half-time score", Score(report.HalfHomeScore, report.HalfAwayScore));
    }

    private byte[] Render(string title, string subtitle, Action<ColumnDescriptor> body) =>
        Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(36);
            page.DefaultTextStyle(text => text.FontSize(10));

            page.Header().Column(header =>
            {
                header.Item().Text(title).FontSize(18).Bold();
                header.Item().Text(subtitle).FontSize(12).FontColor(Colors.Grey.Darken2);
                header.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
            });

            page.Content().PaddingVertical(12).Column(column =>
            {
                column.Spacing(14);
                body(column);
            });

            page.Footer().Row(row =>
            {
                row.RelativeItem().Text($"Times are in {_zone.Id} time.").FontSize(8).FontColor(Colors.Grey.Darken1);
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(style => style.FontSize(8).FontColor(Colors.Grey.Darken1));
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        })).GeneratePdf();

    private static void Section(ColumnDescriptor column, string title, Action<IContainer> content)
    {
        column.Item().Text(title).FontSize(12).SemiBold();
        content(column.Item());
    }

    private static void Facts(IContainer container, IEnumerable<(string Label, string? Value)> facts) =>
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1);
                columns.RelativeColumn(3);
            });
            foreach (var (label, value) in facts)
            {
                table.Cell().PaddingVertical(2).Text(label).FontColor(Colors.Grey.Darken2);
                table.Cell().PaddingVertical(2).Text(value ?? "-");
            }
        });

    private static void Lineup(IContainer container, string teamName, string? formation, IReadOnlyList<LineupPlayerDto> players) =>
        container.Column(column =>
        {
            column.Spacing(2);
            column.Item().Text(formation is null ? teamName : $"{teamName} ({formation})").SemiBold();
            if (players.Count == 0)
            {
                column.Item().Text(NotRecorded);
                return;
            }
            LineupGroup(column, "Starters", players.Where(p => p.IsStarter));
            LineupGroup(column, "Substitutes", players.Where(p => !p.IsStarter));
        });

    private static void LineupGroup(ColumnDescriptor column, string title, IEnumerable<LineupPlayerDto> players)
    {
        column.Item().PaddingTop(4).Text(title).FontSize(9).FontColor(Colors.Grey.Darken2);
        foreach (var player in players)
            column.Item().Text(string.Join(" ", new[] { player.ShirtNumber?.ToString(CultureInfo.InvariantCulture), player.PlayerName, player.PositionName }
                .Where(part => !string.IsNullOrWhiteSpace(part))));
    }

    private static void Table(IContainer container, (string Header, float Width)[] columns, IEnumerable<string?[]> rows) =>
        container.Table(table =>
        {
            table.ColumnsDefinition(definition =>
            {
                foreach (var column in columns)
                    definition.RelativeColumn(column.Width);
            });

            table.Header(header =>
            {
                foreach (var column in columns)
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingVertical(3)
                        .Text(column.Header).FontSize(9).SemiBold();
            });

            foreach (var row in rows)
                foreach (var cell in row)
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3)
                        .Text(string.IsNullOrWhiteSpace(cell) ? "-" : cell).FontSize(9);
        });

    private static string Score(int? home, int? away) => home is null || away is null ? "-" : $"{home} : {away}";

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>Formats a UTC time as local time in the report's zone, e.g. 19.07.2025 22:00.</summary>
    public string DateText(DateTime utc) =>
        Instant.FromDateTimeUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc))
            .InZone(_zone)
            .ToDateTimeUnspecified()
            .ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

    private static string DayText(DateTime date) => date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
}
