using Competition.Application.Common.Models;
using Competition.Application.Queries.City;
using Competition.Application.Queries.City.Export;
using Competition.Application.Queries.City.GetAll;
using Competition.Application.Queries.Stadium;
using Competition.Application.Queries.Stadium.GetAll;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;
using Competition.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CityEntity = Competition.Domain.Entities.City;
using StadiumEntity = Competition.Domain.Entities.MatchEntities.Stadium;

namespace Competition.Tests.UnitTests.ImportExport;

/// <summary>COMP-50: backend column sorting and export of the filtered, sorted list.</summary>
public class SortingAndExportTests
{
    private record Seed(AppDbContext Context, Country Bosnia, Country Croatia);

    private static async Task<Seed> SeedAsync()
    {
        var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("Sort_" + Guid.NewGuid()).Options);
        var bosnia = new Country { Name = "Bosnia and Herzegovina", Slug = "bih" };
        var croatia = new Country { Name = "Croatia", Slug = "croatia" };
        var sarajevo = City("Sarajevo", bosnia, 275_000, capital: true);
        var mostar = City("Mostar", bosnia, 105_000);
        var zagreb = City("Zagreb", croatia, 790_000, capital: true);
        context.Cities.AddRange(sarajevo, mostar, zagreb);
        context.Stadiums.AddRange(
            Stadium("Grbavica", sarajevo, 13_000, 1951),
            Stadium("Asim Ferhatovic Hase", sarajevo, 34_500, 1947),
            Stadium("Bijeli Brijeg", mostar, 9_000, 1971),
            Stadium("Maksimir", zagreb, 35_000, 1912));
        await context.SaveChangesAsync();
        return new Seed(context, bosnia, croatia);
    }

    private static CityEntity City(string name, Country country, int population, bool capital = false) => new()
    {
        Name = name, Slug = name.ToLower(), Country = country, Population = population, IsCapital = capital,
        GeoData = new GeoData { Latitude = 43.85m, Longitude = 18.41m, Description = name + " centre" },
        CountryId = Guid.Empty, GeoDataId = Guid.Empty
    };

    private static StadiumEntity Stadium(string name, CityEntity city, int capacity, int year) => new()
    {
        Name = name, Slug = name.ToLower().Replace(' ', '-'), Capacity = capacity, City = city,
        DateOfConstruction = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc), GeoData = new GeoData()
    };

    private static readonly CityListFilter AllCities = new(null, null, null, null, null);
    private static readonly StadiumListFilter AllStadiums = new(null, null, null, null, null);

    // ---------- Sorting ----------

    [Fact]
    public async Task Cities_DefaultSort_ShouldBeNameAscending()
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;

        var result = await new CityRepository(seed.Context).GetPagedFilteredAsync(AllCities, 1, 10);

        result.Items.Select(c => c.Name).Should().Equal("Mostar", "Sarajevo", "Zagreb");
    }

    [Fact]
    public async Task Cities_PopulationDescending_ShouldPutLargestFirst()
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;

        var result = await new CityRepository(seed.Context)
            .GetPagedFilteredAsync(AllCities with { Sort = new SortSpec("population", "desc") }, 1, 10);

        result.Items.Select(c => c.Name).Should().Equal("Zagreb", "Sarajevo", "Mostar");
    }

    [Fact]
    public async Task Cities_ByCountry_ShouldSortByCountryNameThenCityName()
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;

        var result = await new CityRepository(seed.Context)
            .GetPagedFilteredAsync(AllCities with { Sort = new SortSpec("country", "desc") }, 1, 10);

        result.Items.Select(c => c.Name).Should().Equal("Zagreb", "Mostar", "Sarajevo");
    }

    [Theory]
    [InlineData("capacity", "asc", new[] { "Bijeli Brijeg", "Grbavica", "Asim Ferhatovic Hase", "Maksimir" })]
    [InlineData("capacity", "desc", new[] { "Maksimir", "Asim Ferhatovic Hase", "Grbavica", "Bijeli Brijeg" })]
    [InlineData("dateOfConstruction", "asc", new[] { "Maksimir", "Asim Ferhatovic Hase", "Grbavica", "Bijeli Brijeg" })]
    [InlineData("city", "asc", new[] { "Bijeli Brijeg", "Asim Ferhatovic Hase", "Grbavica", "Maksimir" })]
    [InlineData("name", "desc", new[] { "Maksimir", "Grbavica", "Bijeli Brijeg", "Asim Ferhatovic Hase" })]
    public async Task Stadiums_SortKeys_ShouldOrderAsExpected(string sortBy, string sortDir, string[] expected)
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;

        var result = await new StadiumRepository(seed.Context)
            .GetPagedFilteredAsync(AllStadiums with { Sort = new SortSpec(sortBy, sortDir) }, 1, 10);

        result.Items.Select(s => s.Name).Should().Equal(expected);
    }

    [Fact]
    public async Task Stadiums_SortAppliesBeforePaging()
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;

        var page2 = await new StadiumRepository(seed.Context)
            .GetPagedFilteredAsync(AllStadiums with { Sort = new SortSpec("capacity", "desc") }, 2, 2);

        page2.Items.Select(s => s.Name).Should().Equal("Grbavica", "Bijeli Brijeg");
        page2.TotalCount.Should().Be(4);
    }

    [Theory]
    [InlineData("population", null, true)]
    [InlineData("POPULATION", "DESC", true)]
    [InlineData(null, null, true)]
    [InlineData("capacity", "asc", false)]
    [InlineData("name", "down", false)]
    [InlineData("Id", "asc", false)]
    public void CityValidator_SortParams(string? sortBy, string? sortDir, bool valid)
    {
        new GetAllCitiesValidator().Validate(new GetAllCitiesCommand { SortBy = sortBy, SortDir = sortDir })
            .IsValid.Should().Be(valid);
    }

    [Fact]
    public void StadiumValidator_UnknownSortKey_ShouldFail()
    {
        new GetAllStadiumsQueryValidator().Validate(new GetAllStadiumsQuery { SortBy = "population" })
            .IsValid.Should().BeFalse();
    }

    // ---------- Export ----------

    [Fact]
    public async Task ExportCities_ShouldApplyFilterAndSortAndRoundTrip()
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;
        var files = new TabularFileService();
        var handler = new ExportCitiesQueryHandler(new CityRepository(seed.Context), new PublicIdLookup(seed.Context), files);

        var file = await handler.Handle(new ExportCitiesQuery
        {
            Format = "xlsx", CountryPublicId = seed.Bosnia.PublicId, SortBy = "population", SortDir = "desc"
        }, CancellationToken.None);

        file.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        file.FileName.Should().StartWith("cities-").And.EndWith(".xlsx");
        var rows = await files.ReadAsync(new MemoryStream(file.Content), Application.Interfaces.TabularFormat.Xlsx);
        rows.Select(r => r["Name"]).Should().Equal("Sarajevo", "Mostar");
        rows[0]["Country"].Should().Be("Bosnia and Herzegovina");
        rows[0]["IsCapital"].Should().Be("true");
        rows[0]["Description"].Should().Be("Sarajevo centre");
        rows[0].Keys.Should().NotContain("Id", "internal ids are never exported");
    }

    [Fact]
    public async Task ExportCities_UnknownCountryFilter_ShouldReturnHeaderOnlyCsv()
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;
        var handler = new ExportCitiesQueryHandler(new CityRepository(seed.Context), new PublicIdLookup(seed.Context), new TabularFileService());

        var file = await handler.Handle(new ExportCitiesQuery { CountryPublicId = Guid.NewGuid().ToString("N") }, CancellationToken.None);

        file.ContentType.Should().StartWith("text/csv");
        System.Text.Encoding.UTF8.GetString(file.Content).Trim('﻿', '\r', '\n')
            .Should().Be(string.Join(",", ExportCitiesQueryHandler.Columns));
    }
}
