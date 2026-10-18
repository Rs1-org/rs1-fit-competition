using AutoMapper;
using Competition.Application;
using Competition.Api.Controllers;
using Competition.Application.Commands.ClubCommands;
using Competition.Application.Interfaces;
using Competition.Application.Queries.Clubs;
using Competition.Domain.Entities;
using CityEntity = Competition.Domain.Entities.City;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NodaTime;
using System.Reflection;
using Xunit;

namespace Competition.Tests.UnitTests.Clubs;

public class ClubRulesTests
{
    private static readonly ClubListFilter NoFilter = new(null, null, null, null, null, null, null);
    private static AppDbContext NewContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase("Clubs_" + Guid.NewGuid()).Options);

    [Theory]
    [InlineData("", null, 7)]
    [InlineData("  ", null, 7)]
    [InlineData("!!", null, 7)]
    [InlineData("Valid club", "2999-01-01", 7)]
    [InlineData("Valid club", "2025-02-30", 7)]
    [InlineData("Valid club", "0000-01-01", 7)]
    [InlineData("Valid club", "2025-01-01", 99)]
    public void Create_RejectsInvalidNameDateOrAge(string name, string? date, int age) =>
        new CreateClubCommandValidator(SystemClock.Instance).Validate(new CreateClubCommand
        { Name = name, FoundingDate = date, ClubAge = (ClubAge)age }).IsValid.Should().BeFalse();

    [Fact]
    public void Query_RejectsInvalidPagingDateRangeAndSort()
    {
        var result = new GetAllClubsQueryValidator().Validate(new GetAllClubsQuery
        { Page = 0, PageSize = 101, FoundedFrom = "2025-01-01", FoundedTo = "2024-01-01", SortBy = "anything" });
        result.Errors.Select(e => e.PropertyName).Should().Contain(["Page", "PageSize", "FoundedTo", "SortBy"]);
    }

    [Fact]
    public async Task FiltersAndPaging_AreAppliedBeforeCountingAndDeletedRowsAreHidden()
    {
        await using var db = NewContext();
        var country = new Country { Name = "Bosnia and Herzegovina", Slug = "bosnia" };
        var city = new CityEntity { Name = "Mostar", Slug = "mostar", Country = country, CountryId = country.Id, GeoDataId = Guid.NewGuid() };
        db.Clubs.AddRange(
            new Club { Name = "Club Alpha", Slug = "alpha", City = city, ClubAge = ClubAge.U21, FoundingDate = new LocalDate(1990, 1, 1) },
            new Club { Name = "Club Beta", Slug = "beta", City = city, ClubAge = ClubAge.U21, FoundingDate = new LocalDate(2000, 1, 1) },
            new Club { Name = "Other", Slug = "other", ClubAge = ClubAge.Senior });
        await db.SaveChangesAsync();
        var repository = new ClubRepository(db);
        var filter = NoFilter with { Name = "CLUB", CountryId = country.Id, CityId = city.Id, ClubAge = ClubAge.U21, HasStadium = false };
        var page = await repository.GetPagedFilteredAsync(filter, 2, 1);
        page.TotalCount.Should().Be(2);
        page.Items.Should().ContainSingle(c => c.Slug == "beta");
        var dateFiltered = await repository.GetPagedFilteredAsync(filter with
        { FoundedFrom = new LocalDate(1995, 1, 1), FoundedTo = new LocalDate(2005, 1, 1) }, 1, 10);
        dateFiltered.Items.Should().ContainSingle(c => c.Slug == "beta");
        var beta = await repository.GetBySlugAsync("beta");
        await repository.DeleteAsync(beta!);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        (await repository.GetBySlugAsync("beta")).Should().BeNull();
        (await repository.GetPagedFilteredAsync(NoFilter, 1, 10)).TotalCount.Should().Be(2);
        (await db.Clubs.IgnoreQueryFilters().SingleAsync(c => c.Slug == "beta")).DeletedAt.Should().NotBeNull();
        (await repository.SlugExistsAsync("beta", null)).Should().BeFalse();
    }

    [Fact]
    public async Task CreateAndUpdate_PersistReferencesAndReturnOnlyPublicIdentifiers()
    {
        await using var db = NewContext();
        var firstCity = new CityEntity { Name = "Mostar", Slug = "mostar", CountryId = Guid.NewGuid(), GeoDataId = Guid.NewGuid() };
        var secondCity = new CityEntity { Name = "Sarajevo", Slug = "sarajevo", CountryId = Guid.NewGuid(), GeoDataId = Guid.NewGuid() };
        db.Cities.AddRange(firstCity, secondCity);
        await db.SaveChangesAsync();
        using var services = new ServiceCollection().AddLogging().AddApplicationServices().BuildServiceProvider();
        var mapper = services.GetRequiredService<IMapper>();
        var repository = new ClubRepository(db);
        var lookup = new PublicIdLookup(db);
        var unitOfWork = new UnitOfWork(db);
        var created = await new CreateClubCommandHandler(repository, lookup, unitOfWork, mapper)
            .Handle(new CreateClubCommand { Name = "Old club", CityPublicId = firstCity.PublicId }, CancellationToken.None);
        created.PublicId.Should().HaveLength(32);
        created.CityPublicId.Should().Be(firstCity.PublicId);
        var updated = await new UpdateClubCommandHandler(repository, lookup, unitOfWork, mapper)
            .Handle(new UpdateClubCommand { Slug = created.Slug, Name = "New club", CityPublicId = secondCity.PublicId,
                FoundingDate = "1921-09-19", ClubAge = ClubAge.U21 }, CancellationToken.None);
        updated.PublicId.Should().Be(created.PublicId);
        updated.Slug.Should().Be("new-club");
        updated.CityPublicId.Should().Be(secondCity.PublicId);
        updated.FoundingDate.Should().Be("1921-09-19");
        updated.ClubAge.Should().Be((int)ClubAge.U21);
        db.ChangeTracker.Clear();
        (await repository.GetBySlugAsync("new-club"))!.CityId.Should().Be(secondCity.Id);
    }

    [Fact]
    public async Task Create_UnknownReference_ReturnsFieldValidationAndDoesNotSave()
    {
        var clubs = new Mock<IClub>();
        var lookup = new Mock<IPublicIdLookup>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new CreateClubCommandHandler(clubs.Object, lookup.Object, unitOfWork.Object, Mock.Of<IMapper>());
        var act = () => handler.Handle(new CreateClubCommand { Name = "Club", CityPublicId = BaseEntity.NewPublicId() }, CancellationToken.None);
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().Contain(e => e.PropertyName == "CityPublicId");
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_PropagatesCancellationAndSavesOnce()
    {
        using var cts = new CancellationTokenSource();
        var clubs = new Mock<IClub>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var lookup = new Mock<IPublicIdLookup>();
        var cityPublicId = BaseEntity.NewPublicId();
        lookup.Setup(l => l.GetIdByPublicIdAsync<CityEntity>(cityPublicId, cts.Token)).ReturnsAsync(Guid.NewGuid());
        var handler = new CreateClubCommandHandler(clubs.Object, lookup.Object, unitOfWork.Object, Mock.Of<IMapper>());
        await handler.Handle(new CreateClubCommand { Name = " Club ", CityPublicId = cityPublicId }, cts.Token);
        clubs.Verify(c => c.SlugExistsAsync("club", It.IsAny<Guid?>(), cts.Token), Times.Once);
        lookup.Verify(l => l.GetIdByPublicIdAsync<CityEntity>(cityPublicId, cts.Token), Times.Once);
        clubs.Verify(c => c.AddAsync(It.Is<Club>(x => x.Name == "Club" && x.Slug == "club"), cts.Token), Times.Once);
        clubs.Verify(c => c.GetBySlugAsync("club", cts.Token), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
    }

    [Theory]
    [InlineData(nameof(ClubController.Create), "Admin,Moderator")]
    [InlineData(nameof(ClubController.Update), "Admin,Moderator")]
    [InlineData(nameof(ClubController.Delete), "Admin")]
    public void Writes_RequireExpectedRoles(string action, string roles) =>
        typeof(ClubController).GetMethod(action)!.GetCustomAttribute<AuthorizeAttribute>()!.Roles.Should().Be(roles);
}
