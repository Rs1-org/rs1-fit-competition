using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Competition.Tests.UnitTests.Statistics;

public class StatisticsPersistenceTests
{
    private static AppDbContext NewContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase($"Statistics_{Guid.NewGuid()}").Options);

    [Fact]
    public async Task Statistics_PersistTeamValues_WithPublicIdsAndTimestamps()
    {
        await using var db = NewContext();
        var statistic = NewStatistic();
        db.Statistics.Add(statistic);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var saved = await db.Statistics.Include(x => x.TeamStatistics).SingleAsync();
        saved.PublicId.Should().HaveLength(BaseEntity.PublicIdLength);
        saved.CreatedAt.Should().NotBe(default);
        saved.TeamStatistics.Should().ContainSingle();
        var team = saved.TeamStatistics.Single();
        team.PublicId.Should().HaveLength(BaseEntity.PublicIdLength);
        team.TotalShots.Should().Be(12);
        team.ShotsOnTarget.Should().Be(5);
        team.Possession.Should().Be(60);
        team.SelectionId.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("statistic")]
    [InlineData("match")]
    [InlineData("selection")]
    [InlineData("team")]
    public async Task DeletedEntities_AreHidden_WhileRowsArePreserved(string target)
    {
        await using var db = NewContext();
        var statistic = NewStatistic();
        db.Statistics.Add(statistic);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        db.Remove(target switch
        {
            "statistic" => (BaseEntity)await db.Statistics.SingleAsync(),
            "match" => await db.Matches.SingleAsync(),
            "selection" => await db.Selections.SingleAsync(),
            _ => await db.TeamStatistics.SingleAsync(),
        });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        (await db.TeamStatistics.CountAsync()).Should().Be(0);
        (await db.TeamStatistics.IgnoreQueryFilters().CountAsync()).Should().Be(1);
        if (target is "match" or "statistic")
            (await db.Statistics.CountAsync()).Should().Be(0);
        (await db.Statistics.IgnoreQueryFilters().CountAsync()).Should().Be(1);
    }

    private static Statistic NewStatistic() => new()
    {
        Match = new Match { Slug = $"test-{Guid.NewGuid():N}" },
        TeamStatistics = [new TeamStatistic
        {
            Selection = new Selection { Name = "Home", Abbreviation = "HOM" },
            TotalShots = 12, ShotsOnTarget = 5, TotalYellowCards = 2, TotalRedCards = 0,
            Corners = 4, BigChances = 3, Possession = 60,
        }],
    };
}
