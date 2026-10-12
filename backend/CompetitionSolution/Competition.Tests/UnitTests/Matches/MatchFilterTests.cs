using Competition.Application.DTOs.MatchDto;
using Competition.Application.Queries.Match.GetAll;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NodaTime;
using Xunit;
using CityEntity = Competition.Domain.Entities.City;
using MatchEntity = Competition.Domain.Entities.MatchEntities.Match;

namespace Competition.Tests.UnitTests.Matches;

/// <summary>COMP-77: GetAllMatchesQueryHandler filters and paging against the real repository.</summary>
public class MatchFilterTests : BaseUnitTest
{
    private static readonly DateTime Day1 = new(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc);

    private record Seed(AppDbContext Context, MatchStatus Scheduled, MatchStatus Finished, Stadium Kosevo, Stadium Grbavica);

    private static async Task<Seed> SeedAsync()
    {
        var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("Match_" + Guid.NewGuid()).Options);

        var sarajevo = new CityEntity
        {
            Name = "Sarajevo",
            Slug = "sarajevo",
            Country = new Country { Name = "Bosnia and Herzegovina", Slug = "bosnia-and-herzegovina" },
            GeoData = new GeoData(),
            CountryId = Guid.Empty,
            GeoDataId = Guid.Empty
        };
        var kosevo = new Stadium { Name = "Koševo", Slug = "kosevo", City = sarajevo, GeoData = new GeoData() };
        var grbavica = new Stadium { Name = "Grbavica", Slug = "grbavica", City = sarajevo, GeoData = new GeoData() };
        var scheduled = new MatchStatus { Name = "Scheduled", Slug = "scheduled" };
        var finished = new MatchStatus { Name = "Finished", Slug = "finished" };

        // Added out of date order on purpose, the list must come back sorted by date.
        context.Matches.AddRange(
            NewMatch("m3", Day1.AddDays(2), finished, grbavica),
            NewMatch("m1", Day1, finished, kosevo),
            NewMatch("m4", Day1.AddDays(3), scheduled, kosevo, isPostponed: true),
            NewMatch("m2", Day1.AddDays(1), scheduled, kosevo),
            NewMatch("m5", Day1.AddDays(4), scheduled, grbavica));
        await context.SaveChangesAsync();
        return new Seed(context, scheduled, finished, kosevo, grbavica);
    }

    private static MatchEntity NewMatch(string slug, DateTime date, MatchStatus status, Stadium stadium, bool isPostponed = false) => new()
    {
        Slug = slug,
        Date = Instant.FromDateTimeUtc(date),
        Status = status,
        Stadium = stadium,
        IsPostponed = isPostponed
    };

    private GetAllMatchesQueryHandler NewHandler(AppDbContext context)
    {
        MapperMock
            .Setup(m => m.Map<IReadOnlyList<MatchListDto>>(It.IsAny<object>()))
            .Returns((object source) => ((IEnumerable<MatchEntity>)source)
                .Select(m => new MatchListDto { Id = m.Id, Slug = m.Slug, StatusName = m.Status?.Name, StadiumName = m.Stadium?.Name })
                .ToList());
        return new GetAllMatchesQueryHandler(new MatchRepository(context), Mapper);
    }

    [Fact]
    public async Task NoFilter_ShouldReturnAllSortedByDate()
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;

        var result = await NewHandler(seed.Context).Handle(new GetAllMatchesQuery(), CancellationToken.None);

        result.TotalCount.Should().Be(5);
        result.Items.Select(m => m.Slug).Should().ContainInOrder("m1", "m2", "m3", "m4", "m5");
        result.Items.Should().OnlyContain(m => m.StatusName != null && m.StadiumName != null);
    }

    [Fact]
    public async Task StatusId_ShouldReturnOnlyMatchesWithThatStatus()
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;

        var result = await NewHandler(seed.Context)
            .Handle(new GetAllMatchesQuery { StatusId = seed.Finished.Id }, CancellationToken.None);

        result.Items.Select(m => m.Slug).Should().Equal("m1", "m3");
    }

    [Fact]
    public async Task StadiumId_ShouldReturnOnlyMatchesAtThatStadium()
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;

        var result = await NewHandler(seed.Context)
            .Handle(new GetAllMatchesQuery { StadiumId = seed.Grbavica.Id }, CancellationToken.None);

        result.Items.Select(m => m.Slug).Should().Equal("m3", "m5");
    }

    [Fact]
    public async Task DateRange_ShouldBeInclusiveOnBothEnds()
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;

        var result = await NewHandler(seed.Context).Handle(
            new GetAllMatchesQuery { DateFrom = Day1.AddDays(1), DateTo = Day1.AddDays(3) },
            CancellationToken.None);

        result.Items.Select(m => m.Slug).Should().Equal("m2", "m3", "m4");
    }

    [Fact]
    public async Task CombinedFilters_ShouldIntersect()
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;

        var result = await NewHandler(seed.Context).Handle(
            new GetAllMatchesQuery { StatusId = seed.Scheduled.Id, StadiumId = seed.Kosevo.Id, IsPostponed = false },
            CancellationToken.None);

        result.Items.Should().ContainSingle(m => m.Slug == "m2");
    }

    [Fact]
    public async Task Paging_ShouldApplyAfterFilter()
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;

        var result = await NewHandler(seed.Context).Handle(
            new GetAllMatchesQuery { StatusId = seed.Scheduled.Id, Page = 2, PageSize = 2 },
            CancellationToken.None);

        result.TotalCount.Should().Be(3);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.Items.Should().ContainSingle(m => m.Slug == "m5");
    }

    [Fact]
    public async Task SoftDeletedMatch_ShouldNotBeListed()
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;
        var m1 = await seed.Context.Matches.SingleAsync(m => m.Slug == "m1");
        m1.DeletedAt = SystemClock.Instance.GetCurrentInstant();
        await seed.Context.SaveChangesAsync();

        var result = await NewHandler(seed.Context).Handle(new GetAllMatchesQuery(), CancellationToken.None);

        result.TotalCount.Should().Be(4);
        result.Items.Should().NotContain(m => m.Slug == "m1");
    }
}
