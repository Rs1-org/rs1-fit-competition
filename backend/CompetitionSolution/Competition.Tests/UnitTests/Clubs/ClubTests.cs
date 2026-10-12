using Competition.Application.Commands.ClubCommands.Create;
using Competition.Application.Commands.ClubCommands.Delete;
using Competition.Application.Commands.ClubCommands.Validators;
using Competition.Application.Common.Models;
using Competition.Application.Interfaces;
using Competition.Application.Queries.Club.GetAll;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NodaTime;
using CityEntity = Competition.Domain.Entities.City;

namespace Competition.Tests.UnitTests.Clubs;

public class ClubTests
{
    [Fact]
    public async Task Create_PassesCancellationTokenAndReturnsPublicIds()
    {
        var ct = new CancellationTokenSource().Token;
        var lookup = new Mock<IPublicIdLookup>();
        var clubs = new Mock<IClubRepository>();
        var city = new CityEntity { Id = Guid.NewGuid(), PublicId = BaseEntity.NewPublicId(), Name = "Sarajevo", CountryId = Guid.Empty, GeoDataId = Guid.Empty };
        var stadium = new Stadium { Id = Guid.NewGuid(), PublicId = BaseEntity.NewPublicId(), Name = "Stadium" };
        lookup.Setup(x => x.GetRequiredIdAsync<CityEntity>(city.PublicId, ct)).ReturnsAsync(city.Id);
        lookup.Setup(x => x.GetRequiredIdAsync<Stadium>(stadium.PublicId, ct)).ReturnsAsync(stadium.Id);
        Club? saved = null;
        clubs.Setup(x => x.AddAsync(It.IsAny<Club>(), ct)).Callback<Club, CancellationToken>((x, _) => saved = x).Returns(Task.CompletedTask);
        clubs.Setup(x => x.GetBySlugAsync("test-club", ct)).ReturnsAsync(() =>
        {
            saved!.City = city; saved.Stadium = stadium; return saved;
        });
        var unitOfWork = new Mock<IUnitOfWork>();
        var result = await new CreateClubCommandHandler(clubs.Object, lookup.Object, unitOfWork.Object).Handle(new CreateClubCommand
        { Name = " Test Club ", FoundingDate = "1950-01-01", ClubAge = ClubAge.Senior, CityId = city.PublicId, StadiumId = stadium.PublicId }, ct);
        result.Id.Should().HaveLength(32);
        result.CityId.Should().Be(city.PublicId);
        result.StadiumId.Should().Be(stadium.PublicId);
        result.Name.Should().Be("Test Club");
        clubs.Verify(x => x.AddAsync(It.IsAny<Club>(), ct), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        lookup.VerifyAll();
    }

    [Theory]
    [InlineData("", "1950-01-01", 7)]
    [InlineData("Club", "2050-01-01", 7)]
    [InlineData("Club", "2020-02-30", 7)]
    [InlineData("Club", "1950-01-01", 99)]
    public async Task Validator_RejectsInvalidFields(string name, string date, int age)
    {
        var validator = new CreateClubCommandValidator(Mock.Of<IPublicIdLookup>(), Mock.Of<IStadium>(), Mock.Of<IClubRepository>(), SystemClock.Instance);
        var result = await validator.ValidateAsync(new CreateClubCommand { Name = name, FoundingDate = date, ClubAge = (ClubAge)age });
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void QueryValidator_RejectsUnsafePaging(int page, int pageSize) =>
        new GetAllClubsQueryValidator().Validate(new GetAllClubsQuery { Page = page, PageSize = pageSize }).IsValid.Should().BeFalse();

    [Fact]
    public async Task Repository_FiltersAllFiveFieldsAndExcludesSoftDeletedClubs()
    {
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var city = new CityEntity { Id = Guid.NewGuid(), Name = "Sarajevo", Slug = "sarajevo", CountryId = Guid.Empty, GeoDataId = Guid.Empty };
        var stadium = new Stadium { Id = Guid.NewGuid(), Name = "Stadium", Slug = "stadium", City = city };
        var target = new Club { Id = Guid.NewGuid(), Name = "Target Club", Slug = "target", City = city, Stadium = stadium, FoundingDate = new LocalDate(1950, 1, 1), ClubAge = ClubAge.Senior };
        db.Clubs.AddRange(target,
            new Club { Id = Guid.NewGuid(), Name = "Other", Slug = "other", City = city, Stadium = stadium, FoundingDate = new LocalDate(1960, 1, 1), ClubAge = ClubAge.U7 },
            new Club { Id = Guid.NewGuid(), Name = "Deleted", Slug = "deleted", City = city, Stadium = stadium, FoundingDate = new LocalDate(1950, 1, 1), DeletedAt = SystemClock.Instance.GetCurrentInstant() });
        await db.SaveChangesAsync();
        var repository = new ClubRepository(db);
        var combined = await repository.GetPagedFilteredAsync(new ClubListFilter
        { Name = "TARGET", CityId = city.PublicId, StadiumId = stadium.PublicId, FoundingYear = 1950, ClubAge = ClubAge.Senior }, 1, 10);
        combined.Items.Should().ContainSingle().Which.Id.Should().Be(target.Id);
        foreach (var filter in new[] {
            new ClubListFilter { Name = "missing" }, new ClubListFilter { CityId = BaseEntity.NewPublicId() },
            new ClubListFilter { StadiumId = BaseEntity.NewPublicId() }, new ClubListFilter { FoundingYear = 1900 },
            new ClubListFilter { ClubAge = ClubAge.U21 } })
            (await repository.GetPagedFilteredAsync(filter, 1, 10)).TotalCount.Should().Be(0);
        var paged = await repository.GetPagedFilteredAsync(new ClubListFilter(), 1, 1);
        paged.TotalCount.Should().Be(2);
        paged.Items.Should().ContainSingle();
        (await repository.GetBySlugAsync("deleted")).Should().BeNull();
    }

    [Fact]
    public async Task Delete_SetsTimestampAndPassesTokenWithoutHardDelete()
    {
        var club = new Club { PublicId = BaseEntity.NewPublicId() };
        var ct = new CancellationTokenSource().Token;
        var repo = new Mock<IClubRepository>();
        repo.Setup(x => x.GetByPublicIdAsync(club.PublicId, ct)).ReturnsAsync(club);
        repo.Setup(x => x.UpdateAsync(club, ct)).Returns(Task.CompletedTask);
        var unitOfWork = new Mock<IUnitOfWork>();
        await new DeleteClubCommandHandler(repo.Object, SystemClock.Instance, unitOfWork.Object).Handle(new DeleteClubCommand { Id = club.PublicId }, ct);
        club.DeletedAt.Should().NotBeNull();
        repo.Verify(x => x.UpdateAsync(club, ct), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(ct), Times.Once);
        repo.Verify(x => x.DeleteAsync(It.IsAny<Club>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
