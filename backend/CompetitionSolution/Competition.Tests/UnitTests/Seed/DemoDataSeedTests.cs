using Competition.Infrastructure.Data;
using Competition.Infrastructure.Data.Seed;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Competition.Tests.UnitTests.Seed;

public class DemoDataSeedTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 10, 29, 12, 0);

    private static AppDbContext NewContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase($"DemoSeed_{Guid.NewGuid()}").Options);

    private static async Task<AppDbContext> SeededContext()
    {
        var db = NewContext();
        await DataSeeder.SeedMatchStatusesAsync(db);
        await DataSeeder.SeedDemoDataAsync(db, Now);
        db.ChangeTracker.Clear();
        return db;
    }

    private static async Task<int[]> Counts(AppDbContext db) =>
    [
        await db.Countries.CountAsync(), await db.Cities.CountAsync(), await db.Stadiums.CountAsync(),
        await db.GeoDatas.CountAsync(), await db.Competitions.CountAsync(), await db.Seasons.CountAsync(),
        await db.Rounds.CountAsync(), await db.Clubs.CountAsync(), await db.Selections.CountAsync(),
        await db.Matches.CountAsync(),
    ];

    [Fact]
    public async Task Seed_InsertsRealisticData()
    {
        await using var db = await SeededContext();

        (await Counts(db)).Should().Equal(3, 8, 10, 18, 1, 2, 9, 8, 8, 36);
        (await db.Seasons.CountAsync(x => x.IsCurrent)).Should().Be(1);
        (await db.Cities.CountAsync(x => x.IsCapital)).Should().Be(3);
    }

    [Fact]
    public async Task Seed_RunTwice_DoesNotDuplicateRows()
    {
        await using var db = await SeededContext();
        var before = await Counts(db);

        await DataSeeder.SeedDemoDataAsync(db, Now.Plus(Duration.FromDays(30)));
        db.ChangeTracker.Clear();

        (await Counts(db)).Should().Equal(before);
    }

    [Fact]
    public async Task CitiesAndStadiums_HaveMapCoordinates()
    {
        await using var db = await SeededContext();

        var cities = await db.Cities.Include(x => x.GeoData).ToListAsync();
        var stadiums = await db.Stadiums.Include(x => x.GeoData).Include(x => x.City).ToListAsync();
        cities.Concat<object>(stadiums).Should().HaveCount(18);
        cities.Should().OnlyContain(x => x.GeoData!.Latitude > 40 && x.GeoData.Longitude > 13 && x.Slug != null);
        stadiums.Should().OnlyContain(x => x.GeoData!.Latitude > 40 && x.GeoData.Longitude > 13
            && x.Capacity > 0 && x.City != null && x.DateOfConstruction.Kind == DateTimeKind.Utc);
    }

    [Fact]
    public async Task Matches_AreFinishedInThePast_AndScheduledInTheFuture()
    {
        await using var db = await SeededContext();

        var matches = await db.Matches.Include(x => x.Status).ToListAsync();
        var finished = matches.Where(x => x.Status!.Slug == "finished").ToList();
        var upcoming = matches.Where(x => x.Status!.Slug is "scheduled" or "postponed").ToList();

        finished.Should().NotBeEmpty().And.OnlyContain(x => x.Date < Now && x.HomeScore != null && x.AwayScore != null
            && x.HalfHomeScore <= x.HomeScore && x.HalfAwayScore <= x.AwayScore);
        upcoming.Should().NotBeEmpty().And.OnlyContain(x => x.Date > Now && x.HomeScore == null && x.AwayScore == null);
        (finished.Count + upcoming.Count).Should().Be(matches.Count);
        matches.Where(x => x.IsPostponed).Should().ContainSingle().Which.Status!.Slug.Should().Be("postponed");
    }

    [Fact]
    public async Task Matches_HaveRoundStadiumAndTwoTeams_EachTeamOncePerRound()
    {
        await using var db = await SeededContext();

        var matches = await db.Matches.ToListAsync();
        matches.Should().OnlyContain(x => x.RoundId != null && x.StadiumId != Guid.Empty
            && x.HomeSelectionId != null && x.AwaySelectionId != null && x.HomeSelectionId != x.AwaySelectionId);
        foreach (var round in matches.GroupBy(x => x.RoundId))
            round.SelectMany(x => new[] { x.HomeSelectionId, x.AwaySelectionId }).Should().OnlyHaveUniqueItems();
        matches.Select(x => x.Slug).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void RoundRobin_PairsEveryTeamOnceInAFullCycle()
    {
        var pairs = Enumerable.Range(0, 7).SelectMany(r =>
        {
            var round = DemoDataSeed.RoundRobin(8, r);
            round.Should().HaveCount(4);
            round.SelectMany(p => new[] { p.Home, p.Away }).Should().BeEquivalentTo(Enumerable.Range(0, 8));
            return round.Select(p => (Math.Min(p.Home, p.Away), Math.Max(p.Home, p.Away)));
        }).ToList();

        pairs.Should().HaveCount(28).And.OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task Seed_ReusesExistingRowsBySlug()
    {
        await using var db = NewContext();
        await DataSeeder.SeedMatchStatusesAsync(db);
        db.Countries.Add(new Domain.Entities.Country { Name = "Bosnia and Herzegovina", Slug = "bosnia-and-herzegovina" });
        await db.SaveChangesAsync();

        await DataSeeder.SeedDemoDataAsync(db, Now);

        (await db.Countries.CountAsync(x => x.Slug == "bosnia-and-herzegovina")).Should().Be(1);
    }
}
