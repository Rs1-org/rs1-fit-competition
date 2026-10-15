using Competition.Application.Common.Models;
using Competition.Application.Features.PlayerQueries.GetAll;
using Competition.Domain.Entities.FormationEntities;
using Competition.Domain.Entities.Person;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;

namespace Competition.Tests.UnitTests.Person;

public class PersonListFilterTests
{
    private static readonly PlayerListFilter NoPlayerFilter = new(null, null, null, null, null);
    private static readonly StaffListFilter NoStaffFilter = new(null, null, null, null, null);

    private static AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("Person_" + Guid.NewGuid()).Options);

    [Fact]
    public async Task Players_NamePositionAndAge_ShouldFilterInTheQuery()
    {
        await using var context = NewContext();
        var striker = new Position { Name = "Striker", Slug = "striker" };
        var keeper = new Position { Name = "Goalkeeper", Slug = "goalkeeper" };
        context.Set<Position>().AddRange(striker, keeper);
        context.Players.AddRange(
            Player("Edin", "Dzeko", 1986, striker),
            Player("Miralem", "Pjanic", 1990, striker),
            Player("Ibrahim", "Sehic", 1988, keeper),
            Player("Young", "Talent", 2010, striker),
            Player("Deleted", "Player", 1986, striker, deleted: true));
        await context.SaveChangesAsync();

        var repository = new PlayerRepository(context);

        var byName = await repository.GetPagedAsync(NoPlayerFilter with { Name = "dze" }, 1, 10);
        var byPosition = await repository.GetPagedAsync(NoPlayerFilter with { PositionId = keeper.Id }, 1, 10);
        var byAge = await repository.GetPagedAsync(NoPlayerFilter with { AgeMin = 35, AgeMax = 40 }, 1, 10);
        var page = await repository.GetPagedAsync(NoPlayerFilter, 2, 2);

        byName.Items.Should().ContainSingle(p => p.LastName == "Dzeko");
        byPosition.Items.Should().ContainSingle(p => p.LastName == "Sehic");
        byAge.Items.Select(p => p.LastName).Should().BeEquivalentTo("Dzeko", "Pjanic", "Sehic");
        page.TotalCount.Should().Be(4);
        page.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Managers_CareerStartRange_ShouldExcludeOutsiders()
    {
        await using var context = NewContext();
        context.Managers.AddRange(
            Manager("Sergej", "Barbarez", new LocalDate(2004, 6, 1)),
            Manager("Faruk", "Hadzibegic", new LocalDate(2010, 1, 1)));
        await context.SaveChangesAsync();

        var repository = new ManagerRepository(context);
        var result = await repository.GetPagedAsync(
            NoStaffFilter with { CareerStartFrom = new LocalDate(2000, 1, 1), CareerStartTo = new LocalDate(2005, 12, 31) },
            1,
            10);

        result.Items.Should().ContainSingle(m => m.LastName == "Barbarez");
    }

    [Fact]
    public void Validator_AgeMinGreaterThanAgeMax_ShouldFail()
    {
        var result = new GetAllPlayersQueryValidator().Validate(new GetAllPlayersQuery
        {
            Page = 0,
            PageSize = 200,
            AgeMin = 40,
            AgeMax = 20
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllPlayersQuery.Page));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllPlayersQuery.PageSize));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllPlayersQuery.AgeMax));
    }

    private static Player Player(string first, string last, int year, Position position, bool deleted = false) => new()
    {
        FirstName = first,
        LastName = last,
        DateOfBirth = new LocalDate(year, 6, 1),
        Picture = [],
        Height = 1.8m,
        ShirtNumber = 9,
        Position = position,
        DeletedAt = deleted ? SystemClock.Instance.GetCurrentInstant() : null
    };

    private static Manager Manager(string first, string last, LocalDate careerStart) => new()
    {
        FirstName = first,
        LastName = last,
        DateOfBirth = new LocalDate(1970, 1, 1),
        Picture = [],
        CareerStart = careerStart
    };
}
