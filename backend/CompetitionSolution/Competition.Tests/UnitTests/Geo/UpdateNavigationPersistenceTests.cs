using AutoMapper;
using Competition.Application.Commands.CompetitionCommands.Update;
using Competition.Application.Commands.CountryCommands.Create;
using Competition.Application.Commands.StadiumCommands.Update;
using Competition.Application.Mappings;
using Competition.Application.Services;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;
using Competition.Infrastructure.Services;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using CityEntity = Competition.Domain.Entities.City;
using CompetitionEntity = Competition.Domain.Entities.CompetitionEntities.Competition;
using StadiumEntity = Competition.Domain.Entities.MatchEntities.Stadium;

namespace Competition.Tests.UnitTests.Geo;

/// <summary>COMP-144: update handlers save through the real EF change tracker, and country create works without a picture.</summary>
public class UpdateNavigationPersistenceTests
{
    private static readonly IMapper Mapper = new ServiceCollection()
        .AddLogging()
        .AddAutoMapper((_, _) => { }, typeof(MappingProfile).Assembly)
        .BuildServiceProvider()
        .GetRequiredService<IMapper>();

    private static AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("Comp144_" + Guid.NewGuid()).Options);

    private static CityEntity NewCity(Country country, string name) => new()
    {
        PublicId = BaseEntity.NewPublicId(),
        Name = name,
        Slug = name.ToLowerInvariant(),
        Population = 100_000,
        Country = country,
        GeoData = new GeoData { Latitude = 43.85m, Longitude = 18.41m },
        CountryId = Guid.Empty,
        GeoDataId = Guid.Empty,
    };

    private static async Task<(AppDbContext Context, CityEntity Sarajevo, CityEntity Mostar)> SeedStadiumAsync()
    {
        var context = NewContext();
        var country = new Country { PublicId = BaseEntity.NewPublicId(), Name = "Bosnia and Herzegovina", Slug = "bosnia-and-herzegovina" };
        var sarajevo = NewCity(country, "Sarajevo");
        var mostar = NewCity(country, "Mostar");
        context.Cities.Add(mostar);
        context.Stadiums.Add(new StadiumEntity
        {
            PublicId = BaseEntity.NewPublicId(),
            Name = "Grbavica",
            Slug = "grbavica",
            Capacity = 13_146,
            City = sarajevo,
            GeoData = new GeoData { Latitude = 43.8519m, Longitude = 18.3893m, Description = "Home of FK Zeljeznicar" },
        });
        await context.SaveChangesAsync();
        return (context, sarajevo, mostar);
    }

    private static UpdateStadiumCommandHandler StadiumHandler(AppDbContext context) => new(
        Mapper, new StadiumRepository(context), new GeoDataRepository(context), new UnitOfWork(context),
        Mock.Of<IImageService>(), new PublicIdLookup(context));

    private static UpdateStadiumCommand StadiumUpdate(string cityPublicId) => new()
    {
        Slug = "grbavica",
        Name = "Grbavica",
        Capacity = 15_000,
        DateOfConstruction = new DateTime(1953, 1, 1),
        CityPublicId = cityPublicId,
        Latitude = 43.85m,
        Longitude = 18.39m,
        GeoDescription = "Renovated",
    };

    [Fact]
    public async Task UpdateStadium_SameCity_ShouldSave()
    {
        var (context, sarajevo, _) = await SeedStadiumAsync();
        await using var _ = context;

        var result = await StadiumHandler(context).Handle(StadiumUpdate(sarajevo.PublicId), CancellationToken.None);

        result.Capacity.Should().Be(15_000);
        result.CityName.Should().Be("Sarajevo");
        (await context.Stadiums.SingleAsync()).CityId.Should().Be(sarajevo.Id);
    }

    [Fact]
    public async Task UpdateStadium_OtherCity_ShouldMoveStadiumAndReturnNewCity()
    {
        var (context, _, mostar) = await SeedStadiumAsync();
        await using var _ = context;

        var result = await StadiumHandler(context).Handle(StadiumUpdate(mostar.PublicId), CancellationToken.None);

        result.CityName.Should().Be("Mostar");
        result.CityPublicId.Should().Be(mostar.PublicId);
        (await context.Stadiums.SingleAsync()).CityId.Should().Be(mostar.Id);
    }

    [Fact]
    public async Task UpdateCompetition_WithCountry_ShouldKeepCountry()
    {
        await using var context = NewContext();
        var bih = new Country { PublicId = BaseEntity.NewPublicId(), Name = "Bosnia and Herzegovina", Slug = "bosnia-and-herzegovina" };
        var croatia = new Country { PublicId = BaseEntity.NewPublicId(), Name = "Croatia", Slug = "croatia" };
        context.Countries.Add(croatia);
        context.Competitions.Add(new CompetitionEntity { PublicId = BaseEntity.NewPublicId(), Name = "Premijer liga", Slug = "premijer-liga", Country = bih });
        await context.SaveChangesAsync();
        var handler = new UpdateCompetitionCommandHandler(new CompetitionRepository(context), new PublicIdLookup(context), new UnitOfWork(context), Mapper);

        var same = await handler.Handle(new UpdateCompetitionCommand { Slug = "premijer-liga", Name = "Premijer liga", CountryPublicId = bih.PublicId }, CancellationToken.None);
        same.CountryName.Should().Be("Bosnia and Herzegovina");

        var moved = await handler.Handle(new UpdateCompetitionCommand { Slug = "premijer-liga", Name = "Premijer liga", CountryPublicId = croatia.PublicId }, CancellationToken.None);
        moved.CountryName.Should().Be("Croatia");
        (await context.Competitions.SingleAsync()).CountryId.Should().Be(croatia.Id);

        var cleared = await handler.Handle(new UpdateCompetitionCommand { Slug = "premijer-liga", Name = "Premijer liga" }, CancellationToken.None);
        cleared.CountryName.Should().BeNull();
    }

    [Fact]
    public async Task CreateCountry_WithoutPicture_ShouldSaveWithoutUploading()
    {
        await using var context = NewContext();
        var imageService = new Mock<IImageService>();
        var handler = new CreateCountryCommandHandler(Mapper, new CountryRepository(context), new UnitOfWork(context), imageService.Object);

        await handler.Handle(new CreateCountryCommands { Name = "Montenegro" }, CancellationToken.None);

        var saved = await context.Countries.SingleAsync();
        saved.Name.Should().Be("Montenegro");
        saved.PictureUrl.Should().BeNull();
        imageService.Verify(s => s.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UploadImage_NotAnImage_ShouldThrowValidationException()
    {
        var blob = new Mock<IBlobStorageService>();
        var file = new Mock<IFormFile>();
        file.SetupGet(f => f.ContentType).Returns("text/plain");
        file.SetupGet(f => f.FileName).Returns("notes.txt");

        var act = () => new ImageService(blob.Object).UploadImageAsync(file.Object, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*JPEG, PNG or WEBP*");
        blob.VerifyNoOtherCalls();
    }
}
