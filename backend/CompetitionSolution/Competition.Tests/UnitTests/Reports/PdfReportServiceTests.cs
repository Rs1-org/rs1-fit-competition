using System.Text;
using Competition.Application.DTOs.MatchDto;
using Competition.Application.DTOs.ReportDto;
using Competition.Application.Events;
using Competition.Infrastructure.Services;
using Competition.Infrastructure.Settings;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Competition.Tests.UnitTests.Reports;

public class PdfReportServiceTests
{
    private static readonly DateTime Date = new(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc);
    private readonly PdfReportService _service = Service("Europe/Sarajevo");

    private static PdfReportService Service(string timeZone) =>
        new(Options.Create(new ReportSettings { TimeZone = timeZone }));

    [Theory]
    [InlineData(2025, 7, 19, 20, "19.07.2025 22:00")] // summer time, UTC+2
    [InlineData(2026, 1, 10, 18, "10.01.2026 19:00")] // winter time, UTC+1
    [InlineData(2025, 12, 31, 23, "01.01.2026 00:00")] // crosses midnight into the next day
    public void DateText_ShouldShowUtcTimeInTheConfiguredZone(int year, int month, int day, int hour, string expected) =>
        _service.DateText(new DateTime(year, month, day, hour, 0, 0, DateTimeKind.Utc)).Should().Be(expected);

    [Fact]
    public void DateText_UnknownZone_ShouldFallBackToUtc() =>
        Service("Not/AZone").DateText(new DateTime(2025, 7, 19, 20, 0, 0, DateTimeKind.Utc)).Should().Be("19.07.2025 20:00");

    private static void ShouldBePdf(byte[] content) =>
        Encoding.ASCII.GetString(content, 0, 4).Should().Be("%PDF");

    private static LineupPlayerDto Player(string name, bool starter) =>
        new() { PlayerName = name, IsStarter = starter, ShirtNumber = 7, PositionName = "Forward" };

    [Fact]
    public void MatchReport_WithFullData_ShouldRenderAPdf()
    {
        var goal = new EventLookupDto("t1", "goal", "Goal", 1);
        var half = new EventLookupDto("p1", "first-half", "First half", 1);
        var team = new EventParticipantDto("s1", "Željezničar");

        var content = _service.MatchReport(new MatchReportDto
        {
            HomeName = "Željezničar",
            AwayName = "Čelik",
            Date = Date,
            StatusName = "Finished",
            StadiumName = "Grbavica",
            CompetitionName = "Premijer liga",
            RoundName = "Round 1",
            HomeScore = 2,
            AwayScore = 1,
            HalfHomeScore = 1,
            HalfAwayScore = 0,
            Lineup = new MatchLineupDto
            {
                RefereeName = "Đorđe Šćepanović",
                HomeFormationCode = "4-3-3",
                Home = [Player("Džemal Hadžić", true), Player("Ćiro Blažević", false)],
                Away = [Player("Žarko Čović", true)]
            },
            Events =
            [
                new EventDto("e1", "m1", "Match", goal, half, team, new EventParticipantDto("p1", "Džemal Hadžić"),
                    new EventParticipantDto("p2", "Ćiro Blažević"), 45, 2, null)
            ],
            Statistics = [new TeamStatisticReportDto { SelectionName = "Željezničar", TotalShots = 9, Possession = 55 }]
        });

        ShouldBePdf(content);
    }

    [Fact]
    public void MatchReport_ForAScheduledMatch_ShouldRenderWithoutScoresLineupsEventsOrStatistics()
    {
        var content = _service.MatchReport(new MatchReportDto { HomeName = "TBD", AwayName = "TBD", Date = Date });

        ShouldBePdf(content);
    }

    [Fact]
    public void MatchesReport_ShouldRenderRowsOverSeveralPages()
    {
        var rows = Enumerable.Range(0, 200).Select(i => new MatchesReportRowDto
        {
            Date = Date.AddDays(i),
            HomeName = "Borac",
            AwayName = "Široki Brijeg",
            StatusName = "Scheduled",
            IsPostponed = i % 2 == 0,
            StadiumName = "Pecara",
            CompetitionName = "Premijer liga"
        }).ToList();

        var content = _service.MatchesReport(new MatchesReportDto
        {
            DateFrom = Date,
            DateTo = Date.AddDays(200),
            Filters = ["Stadium: Pecara"],
            Rows = rows,
            TotalCount = 1500
        });

        ShouldBePdf(content);
    }

    [Fact]
    public void MatchesReport_WithNoMatches_ShouldRenderAPdf()
    {
        var content = _service.MatchesReport(new MatchesReportDto { DateFrom = Date, DateTo = Date });

        ShouldBePdf(content);
    }
}
