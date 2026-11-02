using Competition.Application.Common.Models;
using Competition.Application.DTOs.ReportDto;
using Competition.Application.Events;
using Competition.Application.Interfaces;
using Competition.Application.Queries.Reports;
using Competition.Domain.Entities.MatchEntities;
using FluentAssertions;
using Moq;
using NodaTime;
using Xunit;
using CompetitionEntity = Competition.Domain.Entities.CompetitionEntities.Competition;
using MatchEntity = Competition.Domain.Entities.MatchEntities.Match;
using Selection = Competition.Domain.Entities.Selection;

namespace Competition.Tests.UnitTests.Reports;

public class ReportHandlerTests
{
    private static readonly DateTime From = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime To = new(2026, 10, 31, 23, 59, 59, DateTimeKind.Utc);
    private static readonly byte[] Pdf = [1, 2, 3];

    private readonly Mock<IMatch> _matches = new();
    private readonly Mock<IMatchLineup> _lineups = new();
    private readonly Mock<IEventStore> _events = new();
    private readonly Mock<IStatistic> _statistics = new();
    private readonly Mock<IPublicIdLookup> _lookup = new();
    private readonly Mock<IPdfReportService> _pdf = new();

    private GetMatchReportQueryHandler MatchHandler() =>
        new(_matches.Object, _lineups.Object, _events.Object, _statistics.Object, _lookup.Object, _pdf.Object);

    private GetMatchesReportQueryHandler MatchesHandler() => new(_matches.Object, _lookup.Object, _pdf.Object);

    [Fact]
    public async Task MatchReport_UnknownMatch_ShouldThrowKeyNotFound()
    {
        _lookup.Setup(l => l.GetRequiredIdAsync<MatchEntity>("missing", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        var act = () => MatchHandler().Handle(new GetMatchReportQuery("missing"), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _pdf.Verify(p => p.MatchReport(It.IsAny<MatchReportDto>()), Times.Never);
    }

    [Fact]
    public async Task MatchReport_ShouldPassTheDataAndTokenThroughAndPutTheHomeTeamFirst()
    {
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var id = Guid.NewGuid();
        var home = new Selection { Id = Guid.NewGuid(), Name = "Željezničar" };
        var away = new Selection { Id = Guid.NewGuid(), Name = "Sarajevo" };
        var match = new MatchEntity
        {
            Id = id,
            Slug = "derby",
            Date = Instant.FromDateTimeUtc(From),
            HomeSelection = home,
            HomeSelectionId = home.Id,
            AwaySelection = away,
            AwaySelectionId = away.Id,
            HomeScore = 2,
            AwayScore = 1
        };
        var statistic = new Statistic
        {
            TeamStatistics =
            [
                new TeamStatistic { SelectionId = away.Id, Selection = away, TotalShots = 5 },
                new TeamStatistic { SelectionId = home.Id, Selection = home, TotalShots = 9 }
            ]
        };
        _lookup.Setup(l => l.GetRequiredIdAsync<MatchEntity>("m1", token)).ReturnsAsync(id);
        _matches.Setup(m => m.GetByIdWithNavAsync(id, token)).ReturnsAsync(match);
        _lineups.Setup(l => l.GetMatchAsync(id, token)).ReturnsAsync(match);
        _events.Setup(e => e.GetForMatchAsync("m1", token)).ReturnsAsync([]);
        _statistics.Setup(s => s.GetForMatchAsync("m1", token)).ReturnsAsync(statistic);
        MatchReportDto? rendered = null;
        _pdf.Setup(p => p.MatchReport(It.IsAny<MatchReportDto>()))
            .Callback<MatchReportDto>(r => rendered = r).Returns(Pdf);

        var file = await MatchHandler().Handle(new GetMatchReportQuery("m1"), token);

        file.Content.Should().Equal(Pdf);
        file.ContentType.Should().Be("application/pdf");
        file.FileName.Should().Be("match-report-derby.pdf");
        rendered!.HomeName.Should().Be("Željezničar");
        rendered.HomeScore.Should().Be(2);
        rendered.Statistics.Select(s => s.TotalShots).Should().Equal(9, 5);
    }

    [Fact]
    public async Task MatchReport_WithoutTeamsOrStatistics_ShouldStillRender()
    {
        var id = Guid.NewGuid();
        var match = new MatchEntity { Id = id, Date = Instant.FromDateTimeUtc(From) };
        _lookup.Setup(l => l.GetRequiredIdAsync<MatchEntity>("m1", It.IsAny<CancellationToken>())).ReturnsAsync(id);
        _matches.Setup(m => m.GetByIdWithNavAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(match);
        _lineups.Setup(l => l.GetMatchAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(match);
        _events.Setup(e => e.GetForMatchAsync("m1", It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _pdf.Setup(p => p.MatchReport(It.IsAny<MatchReportDto>())).Returns(Pdf);

        await MatchHandler().Handle(new GetMatchReportQuery("m1"), CancellationToken.None);

        _pdf.Verify(p => p.MatchReport(It.Is<MatchReportDto>(r => r.HomeName == "TBD" && r.Statistics.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task MatchesReport_UnknownFilter_ShouldRenderAnEmptyReportWithoutQuerying()
    {
        _lookup.Setup(l => l.GetIdByPublicIdAsync<CompetitionEntity>("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);
        _pdf.Setup(p => p.MatchesReport(It.IsAny<MatchesReportDto>())).Returns(Pdf);

        await MatchesHandler().Handle(
            new GetMatchesReportQuery { DateFrom = From, DateTo = To, CompetitionPublicId = "missing" }, CancellationToken.None);

        _pdf.Verify(p => p.MatchesReport(It.Is<MatchesReportDto>(r => r.Rows.Count == 0 && r.TotalCount == 0)), Times.Once);
        _matches.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MatchesReport_ShouldKeepTheTotalWhenCutOffAndNameTheFilter()
    {
        using var source = new CancellationTokenSource();
        var token = source.Token;
        var competitionId = Guid.NewGuid();
        var stadium = new Stadium { Name = "Koševo" };
        var row = new MatchEntity { Date = Instant.FromDateTimeUtc(From), Stadium = stadium, Status = new MatchStatus { Name = "Finished" } };
        _lookup.Setup(l => l.GetIdByPublicIdAsync<CompetitionEntity>("c1", token)).ReturnsAsync(competitionId);
        _matches.Setup(m => m.GetPagedWithFiltersAsync(
                1, GetMatchesReportQueryHandler.MaxRows,
                new MatchListFilter(null, null, null, true, From, To, null, competitionId, null, null), token))
            .ReturnsAsync(new PagedResult<MatchEntity>([row], 1500, 1, GetMatchesReportQueryHandler.MaxRows));
        MatchesReportDto? rendered = null;
        _pdf.Setup(p => p.MatchesReport(It.IsAny<MatchesReportDto>()))
            .Callback<MatchesReportDto>(r => rendered = r).Returns(Pdf);

        var file = await MatchesHandler().Handle(
            new GetMatchesReportQuery { DateFrom = From, DateTo = To, CompetitionPublicId = "c1", IsPostponed = true }, token);

        file.FileName.Should().Be("matches-report-20261001-20261031.pdf");
        rendered!.TotalCount.Should().Be(1500);
        rendered.Rows.Should().ContainSingle().Which.StadiumName.Should().Be("Koševo");
        rendered.Rows[0].HomeName.Should().Be("TBD");
        rendered.Filters.Should().Equal("Postponed only");
    }
}
