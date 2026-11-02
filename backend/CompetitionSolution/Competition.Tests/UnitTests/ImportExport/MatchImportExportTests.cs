using System.Text;
using Competition.Application.Commands.MatchCommands.Import;
using Competition.Application.Commands.MatchCommands.Validators;
using Competition.Application.Common.Models;
using Competition.Application.Interfaces;
using Competition.Application.Queries.Match;
using Competition.Application.Queries.Match.Export;
using Competition.Application.Queries.Match.GetAll;
using Competition.Domain.Entities;
using Competition.Domain.Entities.CompetitionEntities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;
using Competition.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using NodaTime;
using Xunit;
using CityEntity = Competition.Domain.Entities.City;
using CompetitionEntity = Competition.Domain.Entities.CompetitionEntities.Competition;
using MatchEntity = Competition.Domain.Entities.MatchEntities.Match;

namespace Competition.Tests.UnitTests.ImportExport;

/// <summary>COMP-127: match list phases and sorting, and the match import and export.</summary>
public class MatchImportExportTests
{
    private const string Header = "Date,Status,Stadium,HomeTeam,AwayTeam,Competition,Season,Round,HomeScore,AwayScore,IsPostponed\r\n";
    private static readonly DateTime Day1 = new(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc);

    private static IFormFile Csv(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var file = new Mock<IFormFile>();
        file.Setup(f => f.FileName).Returns("matches.csv");
        file.Setup(f => f.Length).Returns(bytes.Length);
        file.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(bytes));
        return file.Object;
    }

    private static async Task<AppDbContext> SeedAsync()
    {
        var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("MatchImport_" + Guid.NewGuid()).Options);

        var sarajevo = new CityEntity
        {
            Name = "Sarajevo",
            Slug = "sarajevo",
            Country = new Country { Name = "Bosnia and Herzegovina", Slug = "bosnia-and-herzegovina" },
            GeoData = new GeoData(),
            CountryId = Guid.Empty,
            GeoDataId = Guid.Empty
        };
        var bosnia = new Country { Name = "Bosnia", Slug = "bosnia" };
        context.Stadiums.Add(new Stadium { Name = "Koševo", Slug = "kosevo", City = sarajevo, GeoData = new GeoData() });
        context.MatchStatuses.AddRange(
            new MatchStatus { Name = "Scheduled", Slug = "scheduled" },
            new MatchStatus { Name = "Live", Slug = "live" },
            new MatchStatus { Name = "HalfTime", Slug = "half-time" },
            new MatchStatus { Name = "Finished", Slug = "finished" });
        context.Selections.AddRange(
            new Selection { Name = "FK Sarajevo", Slug = "fk-sarajevo", Abbreviation = "SAR", Country = bosnia },
            new Selection { Name = "FK Borac", Slug = "fk-borac", Abbreviation = "BOR", Country = bosnia });
        context.Rounds.Add(new Round
        {
            Name = "Round 1",
            Slug = "round-1",
            Number = 1,
            Season = new Season { Name = "2026/27", Slug = "2026-27", Competition = new CompetitionEntity { Name = "Premijer liga", Slug = "premijer-liga" } }
        });
        await context.SaveChangesAsync();
        return context;
    }

    private static async Task AddMatchAsync(AppDbContext context, string slug, DateTime date, string status, bool postponed = false)
    {
        context.Matches.Add(new MatchEntity
        {
            Slug = slug,
            Date = Instant.FromDateTimeUtc(date),
            Status = await context.MatchStatuses.SingleAsync(s => s.Name == status),
            Stadium = await context.Stadiums.FirstAsync(),
            IsPostponed = postponed
        });
        await context.SaveChangesAsync();
    }

    private static ImportMatchesCommandHandler Handler(AppDbContext c) => new(
        new MatchRepository(c), new MatchStatusRepository(c), new StadiumRepository(c), new SelectionRepository(c),
        new RoundRepository(c), new TabularFileService(), new UnitOfWork(c));

    private static Task<PagedResult<MatchEntity>> List(AppDbContext c, MatchListFilter filter) =>
        new MatchRepository(c).GetPagedWithFiltersAsync(1, 10, filter);

    [Fact]
    public async Task Import_ValidFile_ShouldSaveAllRowsWithSlugAndRound()
    {
        await using var context = await SeedAsync();

        var result = await Handler(context).Handle(new ImportMatchesCommand
        {
            File = Csv(Header
                + "2026-12-01 18:00,Scheduled,Koševo,FK Sarajevo,FK Borac,Premijer liga,2026/27,Round 1,,,false\r\n"
                + "2025-03-01 18:00,Finished,Koševo,FK Borac,FK Sarajevo,,,,2,1,no\r\n"
                + "2026-12-02 18:00,Scheduled,Koševo,,,,,,,,\r\n")
        }, CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Imported.Should().Be(3);
        var saved = await context.Matches.Include(m => m.Round).OrderBy(m => m.Date).ToListAsync();
        saved.Select(m => m.Slug).Should().Equal("fk-borac-vs-fk-sarajevo-2025-03-01", "fk-sarajevo-vs-fk-borac-2026-12-01", "match-2026-12-02");
        saved[0].HomeScore.Should().Be(2);
        saved[1].Round!.Name.Should().Be("Round 1");
        saved[2].HomeSelectionId.Should().BeNull();
    }

    [Fact]
    public async Task Import_OneBadRow_ShouldSaveNothingAndReportEachColumn()
    {
        await using var context = await SeedAsync();

        var result = await Handler(context).Handle(new ImportMatchesCommand
        {
            File = Csv(Header
                + "2026-12-01 18:00,Scheduled,Koševo,FK Sarajevo,FK Borac,,,,,,\r\n"
                + "2026-12-01 18:00,Unknown,Nowhere,FK Sarajevo,Ghosts,Premijer liga,2026/27,Round 9,,,\r\n"
                + "soon,Scheduled,Koševo,FK Sarajevo,FK Borac,,,,,,\r\n")
        }, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Imported.Should().Be(0);
        result.Errors.Where(e => e.Row == 3).Select(e => e.Column).Should().BeEquivalentTo("Status", "Stadium", "AwayTeam", "Round");
        result.Errors.Should().ContainSingle(e => e.Row == 4).Which.Column.Should().Be("Date");
        (await context.Matches.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Import_SameTeamsAndTime_ShouldBeRejectedAsDuplicate()
    {
        await using var context = await SeedAsync();
        var row = "2026-12-01 18:00,Scheduled,Koševo,FK Sarajevo,FK Borac,,,,,,\r\n";
        await Handler(context).Handle(new ImportMatchesCommand { File = Csv(Header + row) }, CancellationToken.None);

        var result = await Handler(context).Handle(new ImportMatchesCommand { File = Csv(Header + row) }, CancellationToken.None);

        result.Errors.Should().ContainSingle().Which.Message.Should().Contain("already exists");
        (await context.Matches.CountAsync()).Should().Be(1);
    }

    [Fact]
    public void ImportRow_ShouldRejectBadScoresAndEqualTeams()
    {
        var validator = new MatchImportRowValidator();

        validator.Validate(new MatchImportRow("Finished", "Koševo", "A", "B", 2, 1, 1, 1, 0)).IsValid.Should().BeTrue();
        validator.Validate(new MatchImportRow("Finished", "Koševo", "A", "a", null, null, null, null, 0)).IsValid.Should().BeFalse();
        validator.Validate(new MatchImportRow("Finished", "Koševo", "A", "B", 2, null, null, null, 0)).IsValid.Should().BeFalse();
        validator.Validate(new MatchImportRow("Finished", "Koševo", "A", "B", 1, 1, 2, 0, 0)).IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Export_ShouldKeepPhaseAndSortAndBeReadableByTheImport()
    {
        await using var context = await SeedAsync();
        await AddMatchAsync(context, "old", Day1, "Finished");
        await AddMatchAsync(context, "new", Day1.AddDays(1), "Finished");
        await AddMatchAsync(context, "other", Day1.AddDays(2), "Scheduled");
        var files = new TabularFileService();

        var file = await new ExportMatchesQueryHandler(new MatchRepository(context), new PublicIdLookup(context), files)
            .Handle(new ExportMatchesQuery { Format = "csv", Phase = "results", SortBy = "date", SortDir = "desc" }, CancellationToken.None);

        var rows = await files.ReadAsync(new MemoryStream(file.Content), TabularFormat.Csv, CancellationToken.None);
        rows.Select(r => r["Date"]).Should().Equal("2026-10-02 18:00", "2026-10-01 18:00");
        file.FileName.Should().StartWith("matches-");
    }

    [Theory]
    [InlineData("live", new[] { "m-live", "m-half" })]
    [InlineData("results", new[] { "m-done" })]
    [InlineData("upcoming", new[] { "m-next" })]
    public async Task Phase_ShouldCoverItsStatusesAndSkipPostponed(string phase, string[] expected)
    {
        await using var context = await SeedAsync();
        await AddMatchAsync(context, "m-live", Day1, "Live");
        await AddMatchAsync(context, "m-half", Day1.AddHours(1), "HalfTime");
        await AddMatchAsync(context, "m-done", Day1.AddHours(2), "Finished");
        await AddMatchAsync(context, "m-next", Day1.AddHours(3), "Scheduled");
        await AddMatchAsync(context, "m-ppd", Day1.AddHours(4), "Scheduled", postponed: true);

        var result = await List(context, new MatchListFilter(Phase: phase));

        result.Items.Select(m => m.Slug).Should().Equal(expected);
    }

    [Fact]
    public async Task Sort_ShouldDefaultToOldestFirstAndHonourTheRequest()
    {
        await using var context = await SeedAsync();
        await AddMatchAsync(context, "first", Day1, "Finished");
        await AddMatchAsync(context, "second", Day1.AddDays(1), "Finished");

        (await List(context, new MatchListFilter())).Items.Select(m => m.Slug).Should().Equal("first", "second");
        (await List(context, new MatchListFilter(Sort: new SortSpec("date", "desc")))).Items.Select(m => m.Slug).Should().Equal("second", "first");
    }

    [Fact]
    public void Validators_ShouldRejectUnknownPhaseAndSortKey()
    {
        var validator = new GetAllMatchesQueryValidator();

        validator.Validate(new GetAllMatchesQuery { Phase = "live", SortBy = "date", SortDir = "desc" }).IsValid.Should().BeTrue();
        validator.Validate(new GetAllMatchesQuery()).IsValid.Should().BeTrue();
        validator.Validate(new GetAllMatchesQuery { Phase = "soon" }).IsValid.Should().BeFalse();
        validator.Validate(new GetAllMatchesQuery { SortBy = "id" }).IsValid.Should().BeFalse();
    }
}
