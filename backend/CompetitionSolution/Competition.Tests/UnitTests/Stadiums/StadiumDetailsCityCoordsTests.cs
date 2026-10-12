using AutoMapper;
using Competition.Application.DTOs.StadiumDto;
using Competition.Application.Mappings;
using Competition.Application.Queries.Stadium.GetBySlug;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using CityEntity = Competition.Domain.Entities.City;
using StadiumEntity = Competition.Domain.Entities.MatchEntities.Stadium;

namespace Competition.Tests.UnitTests.Stadiums;

/// <summary>COMP-120: stadium details carry the city centre coordinates for the distance on the map.</summary>
public class StadiumDetailsCityCoordsTests
{
    private static readonly IMapper Mapper = new ServiceCollection()
        .AddLogging()
        .AddAutoMapper((_, _) => { }, typeof(MappingProfile).Assembly)
        .BuildServiceProvider()
        .GetRequiredService<IMapper>();

    private static AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("StadiumDetails_" + Guid.NewGuid()).Options);

    private static async Task<AppDbContext> SeedAsync(GeoData cityGeo)
    {
        var context = NewContext();
        var city = new CityEntity
        {
            Name = "Sarajevo",
            Slug = "sarajevo",
            Country = new Country { Name = "Bosnia and Herzegovina", Slug = "bosnia-and-herzegovina" },
            GeoData = cityGeo,
            CountryId = Guid.Empty,
            GeoDataId = Guid.Empty
        };
        context.Stadiums.Add(new StadiumEntity
        {
            Name = "Asim Ferhatović Hase",
            Slug = "kosevo",
            Capacity = 34_500,
            City = city,
            GeoData = new GeoData { Latitude = 43.8686m, Longitude = 18.4093m }
        });
        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task Handle_ShouldReturnStadiumAndCityCentreCoordinates()
    {
        await using var context = await SeedAsync(new GeoData { Latitude = 43.8563m, Longitude = 18.4131m });
        var handler = new GetStadiumBySlugQueryHandler(new StadiumRepository(context), Mapper);

        var result = await handler.Handle(new GetStadiumBySlugQuery { Slug = "kosevo" }, CancellationToken.None);

        result.Latitude.Should().Be(43.8686m);
        result.Longitude.Should().Be(18.4093m);
        result.CityLatitude.Should().Be(43.8563m);
        result.CityLongitude.Should().Be(18.4131m);
    }

    [Fact]
    public void Map_CityGeoDataNotLoaded_ShouldReturnNullCityCoordinates()
    {
        var stadium = new StadiumEntity
        {
            Name = "Grbavica",
            City = new CityEntity { Name = "Sarajevo", CountryId = Guid.Empty, GeoDataId = Guid.Empty }
        };

        var result = Mapper.Map<StadiumDetailsDto>(stadium);

        result.CityLatitude.Should().BeNull();
        result.CityLongitude.Should().BeNull();
    }
}
