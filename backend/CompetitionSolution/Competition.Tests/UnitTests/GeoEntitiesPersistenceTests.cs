using Competition.Application.Commands.CountryCommands.Update;
using Competition.Application.DTOs.CountryDto;
using Competition.Application.Interfaces;
using Competition.Application.Services;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using CityEntity = Competition.Domain.Entities.City;

namespace Competition.Tests.UnitTests;

/// <summary>COMP-74: one save per command, soft-deleted Country/City/Stadium hidden, slug follows the name.</summary>
public class GeoEntitiesPersistenceTests : BaseUnitTest
{
    private static AppDbContext NewContext(string name) =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(name).Options);

    [Fact]
    public async Task RepositoryAdd_ShouldNotWriteUntilUnitOfWorkSaves()
    {
        // Arrange
        var dbName = "Atomic_" + Guid.NewGuid();
        await using var context = NewContext(dbName);
        var repository = new Repository<Country>(context);
        var unitOfWork = new UnitOfWork(context);

        // Act: stage only
        await repository.AddAsync(new Country { Name = "Montenegro", Slug = "montenegro" });

        // Assert: nothing in the store yet (a fresh context sees only saved data)
        await using (var probe = NewContext(dbName))
            (await probe.Countries.CountAsync()).Should().Be(0);

        // Act: the single save of the command
        await unitOfWork.SaveChangesAsync();

        await using (var probe = NewContext(dbName))
            (await probe.Countries.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task SoftDeletedCountryCityStadium_ShouldBeHiddenFromQueries()
    {
        // Arrange
        await using var context = NewContext("SoftDelete_" + Guid.NewGuid());
        var country = new Country { Name = "Bosnia and Herzegovina", Slug = "bosnia-and-herzegovina" };
        var city = new CityEntity { Name = "Sarajevo", Slug = "sarajevo", Country = country, GeoData = new GeoData(), CountryId = Guid.Empty, GeoDataId = Guid.Empty };
        var stadium = new Stadium { Name = "Koševo", Slug = "kosevo", City = city, GeoData = new GeoData() };
        context.Stadiums.Add(stadium);
        await context.SaveChangesAsync();

        // Act: soft delete (SaveChanges turns Remove into DeletedAt)
        context.Stadiums.Remove(stadium);
        context.Cities.Remove(city);
        context.Countries.Remove(country);
        await context.SaveChangesAsync();

        // Assert
        (await context.Countries.CountAsync()).Should().Be(0);
        (await context.Cities.CountAsync()).Should().Be(0);
        (await context.Stadiums.CountAsync()).Should().Be(0);
        (await context.Countries.IgnoreQueryFilters().CountAsync()).Should().Be(1, "rows are kept, only hidden");
        (await context.Cities.IgnoreQueryFilters().CountAsync()).Should().Be(1);
        (await context.Stadiums.IgnoreQueryFilters().CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task UpdateCountry_Rename_ShouldRegenerateSlug()
    {
        // Arrange
        var country = new Country { Name = "Bosnia", Slug = "bosnia" };
        var countryRepository = new Mock<ICountry>();
        countryRepository.Setup(r => r.GetBySlugAsync("bosnia", It.IsAny<CancellationToken>())).ReturnsAsync(country);
        MapperMock.Setup(m => m.Map<CountryNameDto>(It.IsAny<Country>())).Returns(new CountryNameDto());
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new UpdateCountryCommandHandler(Mapper, countryRepository.Object, unitOfWork.Object, Mock.Of<IImageService>(), NullLogger<UpdateCountryCommandHandler>.Instance);

        // Act
        await handler.Handle(new UpdateCountryCommands { Slug = "bosnia", Name = "Bosnia and Herzegovina" }, CancellationToken.None);

        // Assert
        country.Slug.Should().Be("bosnia-and-herzegovina");
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
