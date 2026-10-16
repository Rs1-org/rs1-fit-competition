using Competition.Application.Common.Models;
using Competition.Application.Queries.City;
using Competition.Application.Queries.City.GetAll;
using Competition.Application.Queries.Country;
using Competition.Application.Queries.Country.GetAll;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CityEntity = Competition.Domain.Entities.City;

namespace Competition.Tests.UnitTests.Geo;

/// <summary>COMP-70: 5-param backend filters and query validation for the Country and City lists.</summary>
public class CountryCityFilterTests
{
    private static readonly CountryListFilter NoCountryFilter = new(null, null, null, null, null);
    private static readonly CityListFilter NoCityFilter = new(null, null, null, null, null);

    private static AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("Filter_" + Guid.NewGuid()).Options);

    private static async Task<(AppDbContext Context, Country Bosnia, Country Croatia)> SeedAsync()
    {
        var context = NewContext();
        var bosnia = new Country { Name = "Bosnia and Herzegovina", Slug = "bosnia-and-herzegovina", PictureUrl = "https://img/ba.png" };
        var croatia = new Country { Name = "Croatia", Slug = "croatia" };
        var serbia = new Country { Name = "Serbia", Slug = "serbia", PictureUrl = "" };
        context.Countries.AddRange(bosnia, croatia, serbia);
        context.Cities.AddRange(
            NewCity("Sarajevo", bosnia, 275_000, isCapital: true),
            NewCity("Mostar", bosnia, 105_000, isCapital: false),
            NewCity("Zagreb", croatia, 770_000, isCapital: true),
            NewCity("Split", croatia, 160_000, isCapital: false));
        await context.SaveChangesAsync();
        return (context, bosnia, croatia);
    }

    private static CityEntity NewCity(string name, Country country, int population, bool isCapital) => new()
    {
        Name = name,
        Slug = name.ToLower(),
        Population = population,
        IsCapital = isCapital,
        Country = country,
        GeoData = new GeoData(),
        CountryId = Guid.Empty,
        GeoDataId = Guid.Empty
    };

    // ---------- Country repository ----------

    [Fact]
    public async Task Countries_NoFilter_ShouldReturnAllSortedByName()
    {
        var (context, _, _) = await SeedAsync();
        await using var _ = context;

        var result = await new CountryRepository(context).GetPagedFilteredAsync(NoCountryFilter, 1, 10);

        result.TotalCount.Should().Be(3);
        result.Items.Select(c => c.Name).Should().ContainInOrder("Bosnia and Herzegovina", "Croatia", "Serbia");
    }

    [Fact]
    public async Task Countries_NameAndSlug_ShouldMatchCaseInsensitivePartial()
    {
        var (context, _, _) = await SeedAsync();
        await using var _ = context;
        var repository = new CountryRepository(context);

        (await repository.GetPagedFilteredAsync(NoCountryFilter with { Name = "  CROAT " }, 1, 10))
            .Items.Should().ContainSingle(c => c.Name == "Croatia");
        (await repository.GetPagedFilteredAsync(NoCountryFilter with { Slug = "herzeg" }, 1, 10))
            .Items.Should().ContainSingle(c => c.Slug == "bosnia-and-herzegovina");
    }

    [Fact]
    public async Task Countries_HasPicture_ShouldTreatEmptyUrlAsNoPicture()
    {
        var (context, _, _) = await SeedAsync();
        await using var _ = context;
        var repository = new CountryRepository(context);

        (await repository.GetPagedFilteredAsync(NoCountryFilter with { HasPicture = true }, 1, 10))
            .Items.Select(c => c.Name).Should().BeEquivalentTo("Bosnia and Herzegovina");
        (await repository.GetPagedFilteredAsync(NoCountryFilter with { HasPicture = false }, 1, 10))
            .Items.Select(c => c.Name).Should().BeEquivalentTo("Croatia", "Serbia");
    }

    [Fact]
    public async Task Countries_CreatedRange_ShouldIncludeWholeEndDay()
    {
        var (context, _, _) = await SeedAsync();
        await using var _ = context;
        var repository = new CountryRepository(context);
        var today = DateTime.UtcNow.Date;

        (await repository.GetPagedFilteredAsync(NoCountryFilter with { CreatedFrom = today, CreatedTo = today }, 1, 10))
            .TotalCount.Should().Be(3);
        (await repository.GetPagedFilteredAsync(NoCountryFilter with { CreatedFrom = today.AddDays(1) }, 1, 10))
            .TotalCount.Should().Be(0);
        (await repository.GetPagedFilteredAsync(NoCountryFilter with { CreatedTo = today.AddDays(-1) }, 1, 10))
            .TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Countries_SoftDeleted_ShouldBeExcluded()
    {
        var (context, _, _) = await SeedAsync();
        await using var _ = context;
        context.Countries.Remove(await context.Countries.SingleAsync(c => c.Name == "Serbia"));
        await context.SaveChangesAsync();

        var result = await new CountryRepository(context).GetPagedFilteredAsync(NoCountryFilter, 1, 10);

        result.TotalCount.Should().Be(2);
        result.Items.Should().NotContain(c => c.Name == "Serbia");
    }

    // ---------- City repository ----------

    [Fact]
    public async Task Cities_CountryAndCapital_ShouldCombine()
    {
        var (context, bosnia, _) = await SeedAsync();
        await using var _ = context;

        var result = await new CityRepository(context).GetPagedFilteredAsync(
            NoCityFilter with { CountryId = bosnia.Id, IsCapital = true }, 1, 10);

        result.Items.Should().ContainSingle(c => c.Name == "Sarajevo");
    }

    [Fact]
    public async Task Cities_PopulationRangeAndName_ShouldFilter()
    {
        var (context, _, _) = await SeedAsync();
        await using var _ = context;
        var repository = new CityRepository(context);

        (await repository.GetPagedFilteredAsync(NoCityFilter with { MinPopulation = 150_000, MaxPopulation = 300_000 }, 1, 10))
            .Items.Select(c => c.Name).Should().BeEquivalentTo("Sarajevo", "Split");
        (await repository.GetPagedFilteredAsync(NoCityFilter with { Name = "zag" }, 1, 10))
            .Items.Should().ContainSingle(c => c.Name == "Zagreb");
    }

    [Fact]
    public async Task Cities_Paging_ShouldKeepTotalCountOfFilteredSet()
    {
        var (context, _, _) = await SeedAsync();
        await using var _ = context;

        var result = await new CityRepository(context).GetPagedFilteredAsync(NoCityFilter with { IsCapital = false }, 2, 1);

        result.TotalCount.Should().Be(2);
        result.Items.Should().ContainSingle(c => c.Name == "Split");
    }

    // ---------- Validators ----------

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void GetAllCountries_InvalidPaging_ShouldFail(int page, int pageSize)
    {
        var result = new GetAllCountriesValidator().Validate(new GetAllCountriesCommand { Page = page, PageSize = pageSize });

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void GetAllCountries_CreatedToBeforeFrom_ShouldFail()
    {
        var result = new GetAllCountriesValidator().Validate(new GetAllCountriesCommand
        {
            CreatedFrom = new DateTime(2026, 5, 2),
            CreatedTo = new DateTime(2026, 5, 1)
        });

        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllCountriesCommand.CreatedTo));
    }

    [Fact]
    public void GetAllCountries_ValidFilters_ShouldPass()
    {
        var result = new GetAllCountriesValidator().Validate(new GetAllCountriesCommand
        {
            Name = "Bos",
            HasPicture = true,
            CreatedFrom = new DateTime(2026, 5, 1),
            CreatedTo = new DateTime(2026, 5, 1)
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetAllCities_MaxBelowMinPopulation_ShouldFail()
    {
        var result = new GetAllCitiesValidator().Validate(new GetAllCitiesCommand { MinPopulation = 1000, MaxPopulation = 10 });

        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllCitiesCommand.MaxPopulation));
    }

    [Fact]
    public void GetAllCities_NegativePopulationOrEmptyCountry_ShouldFail()
    {
        var result = new GetAllCitiesValidator().Validate(new GetAllCitiesCommand { MinPopulation = -1, CountryPublicId = "" });

        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllCitiesCommand.MinPopulation));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllCitiesCommand.CountryPublicId));
    }

    [Fact]
    public void GetAllCities_ValidFilters_ShouldPass()
    {
        var result = new GetAllCitiesValidator().Validate(new GetAllCitiesCommand
        {
            Name = "Sar",
            CountryPublicId = Guid.NewGuid().ToString("N"),
            IsCapital = true,
            MinPopulation = 0,
            MaxPopulation = 500_000
        });

        result.IsValid.Should().BeTrue();
    }
}
