using Competition.Application.Interfaces;
using Competition.Application.Queries.Statistics;
using Competition.Domain.Entities;
using Competition.Domain.Entities.CompetitionEntities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Match = Competition.Domain.Entities.MatchEntities.Match;
using CompetitionEntity = Competition.Domain.Entities.CompetitionEntities.Competition;

namespace Competition.Tests.UnitTests.Statistics;

public class StatisticChartsTests
{
    private static ChartMatchRow Row(int? round, string home, int homeScore, string away, int awayScore) =>
        new(round, round.HasValue ? $"Round {round}" : null, home, home, homeScore, away, away, awayScore);

    [Fact]
    public void GoalsPerRound_SumsGoalsOrderedByRound_AndSkipsMatchesWithoutRound()
    {
        var result = GetStatisticChartsQueryHandler.GoalsPerRound(
        [
            Row(2, "A", 1, "B", 1), Row(1, "A", 3, "C", 0), Row(1, "B", 2, "D", 2), Row(null, "C", 5, "D", 5),
        ]);

        result.Select(x => (x.RoundNumber, x.Matches, x.Goals)).Should().Equal((1, 2, 7), (2, 1, 2));
    }

    [Fact]
    public void TeamResults_CountsWinsDrawsLosses_AndOrdersByPoints()
    {
        var result = GetStatisticChartsQueryHandler.TeamResults(
        [
            Row(1, "A", 2, "B", 0), Row(2, "B", 1, "A", 1), Row(3, "C", 0, "A", 3),
        ]);

        var a = result[0];
        a.SelectionName.Should().Be("A");
        (a.Played, a.Wins, a.Draws, a.Losses, a.GoalsFor, a.GoalsAgainst, a.Points).Should().Be((3, 2, 1, 0, 6, 1, 7));
        result[1].SelectionName.Should().Be("B");
        (result[1].Wins, result[1].Draws, result[1].Losses).Should().Be((0, 1, 1));
        result.Should().HaveCount(3);
    }

    [Fact]
    public void TeamResults_IsCappedAtMaxTeams()
    {
        var rows = Enumerable.Range(0, 12).Select(i => Row(1, $"H{i}", 1, $"A{i}", 0));
        GetStatisticChartsQueryHandler.TeamResults(rows).Should().HaveCount(GetStatisticChartsQueryHandler.MaxTeams);
    }

    [Fact]
    public void TeamShots_AveragesPerTeam()
    {
        var result = GetStatisticChartsQueryHandler.TeamShots(
        [
            new ChartTeamShotRow("A", "A", 10, 4, 60), new ChartTeamShotRow("A", "A", 15, 5, 55),
            new ChartTeamShotRow("B", "B", 8, 2, 40),
        ]);

        result[0].Should().Be(new Application.DTOs.StatisticDto.TeamShotsDto("A", "A", 2, 12.5, 4.5, 57.5));
        result[1].SelectionName.Should().Be("B");
    }

    [Fact]
    public async Task Handler_PassesFilterAndCancellationToken_ToRepository()
    {
        var repo = new Mock<IStatistic>();
        using var cts = new CancellationTokenSource();
        repo.Setup(r => r.GetChartMatchesAsync(It.IsAny<ChartFilter>(), cts.Token)).ReturnsAsync([]);
        repo.Setup(r => r.GetChartTeamShotsAsync(It.IsAny<ChartFilter>(), cts.Token)).ReturnsAsync([]);
        var season = BaseEntity.NewPublicId();

        var result = await new GetStatisticChartsQueryHandler(repo.Object)
            .Handle(new GetStatisticChartsQuery { SeasonPublicId = season }, cts.Token);

        result.GoalsPerRound.Should().BeEmpty();
        result.TeamResults.Should().BeEmpty();
        repo.Verify(r => r.GetChartMatchesAsync(new ChartFilter(null, season), cts.Token), Times.Once);
        repo.Verify(r => r.GetChartTeamShotsAsync(new ChartFilter(null, season), cts.Token), Times.Once);
    }

    [Theory]
    [InlineData(null, null, true)]
    [InlineData("not-a-valid-id", null, false)]
    [InlineData(null, "bad", false)]
    public void Validator_AcceptsEmptyFilters_AndRejectsBadPublicIds(string? competition, string? season, bool valid)
    {
        var result = new GetStatisticChartsQueryValidator()
            .Validate(new GetStatisticChartsQuery { CompetitionPublicId = competition, SeasonPublicId = season });
        result.IsValid.Should().Be(valid);
    }

    [Fact]
    public void Validator_AcceptsGeneratedPublicIds()
    {
        new GetStatisticChartsQueryValidator()
            .Validate(new GetStatisticChartsQuery { CompetitionPublicId = BaseEntity.NewPublicId(), SeasonPublicId = BaseEntity.NewPublicId() })
            .IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_ReturnsOnlyFinishedScoredMatches_InTheSelectedSeason()
    {
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"Charts_{Guid.NewGuid()}").Options);
        var finished = new MatchStatus { Name = "Finished", Slug = "finished" };
        var scheduled = new MatchStatus { Name = "Scheduled", Slug = "scheduled" };
        var competition = new CompetitionEntity { Name = "League", Slug = "league" };
        var season = new Season { Name = "2025/26", Slug = "2025-26", Competition = competition };
        var otherSeason = new Season { Name = "2024/25", Slug = "2024-25", Competition = competition };
        var round = new Round { Name = "Round 1", Slug = "round-1", Number = 1, Season = season };
        var otherRound = new Round { Name = "Round 1", Slug = "old-round-1", Number = 1, Season = otherSeason };
        var home = new Selection { Name = "Home", Abbreviation = "HOM" };
        var away = new Selection { Name = "Away", Abbreviation = "AWA" };
        Match NewMatch(string slug, MatchStatus status, Round r, int? homeScore) => new()
        {
            Slug = slug, Status = status, Round = r, HomeSelection = home, AwaySelection = away,
            HomeScore = homeScore, AwayScore = homeScore is null ? null : 1,
        };
        var counted = NewMatch("counted", finished, round, 2);
        var deleted = NewMatch("deleted", finished, round, 3);
        db.Matches.AddRange(counted, deleted, NewMatch("scheduled", scheduled, round, null),
            NewMatch("no-score", finished, round, null), NewMatch("other-season", finished, otherRound, 4));
        db.Statistics.Add(new Statistic { Match = counted, TeamStatistics =
            [new TeamStatistic { Selection = home, TotalShots = 9, ShotsOnTarget = 3, Possession = 52 }] });
        await db.SaveChangesAsync();
        db.Matches.Remove(deleted);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var repo = new StatisticRepository(db);
        var filter = new ChartFilter(competition.PublicId, season.PublicId);
        var matches = await repo.GetChartMatchesAsync(filter);
        var shots = await repo.GetChartTeamShotsAsync(filter);

        matches.Should().ContainSingle().Which.Should().Be(
            new ChartMatchRow(1, "Round 1", home.PublicId, "Home", 2, away.PublicId, "Away", 1));
        shots.Should().ContainSingle().Which.TotalShots.Should().Be(9);
        (await repo.GetChartMatchesAsync(new ChartFilter(null, null))).Should().HaveCount(2);
    }
}
