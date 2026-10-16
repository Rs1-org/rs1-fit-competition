using System.Text;
using Competition.Application.Commands.CityCommands.Import;
using Competition.Application.Commands.CountryCommands.Import;
using Competition.Application.Commands.CountryCommands.Validators;
using Competition.Application.Commands.StadiumCommands.Import;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;
using Competition.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CityEntity = Competition.Domain.Entities.City;

namespace Competition.Tests.UnitTests.ImportExport;

/// <summary>COMP-50: all-or-nothing CSV import with a row-level error report.</summary>
public class ImportHandlerTests
{
    private static AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("Import_" + Guid.NewGuid()).Options);

    private static IFormFile Csv(string content, string fileName = "data.csv")
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var file = new Mock<IFormFile>();
        file.Setup(f => f.FileName).Returns(fileName);
        file.Setup(f => f.Length).Returns(bytes.Length);
        file.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(bytes));
        return file.Object;
    }

    private static async Task<AppDbContext> SeedBosniaAsync()
    {
        var context = NewContext();
        var bosnia = new Country { Name = "Bosnia and Herzegovina", Slug = "bosnia-and-herzegovina" };
        context.Cities.Add(new CityEntity
        {
            Name = "Sarajevo", Slug = "sarajevo", Country = bosnia, GeoData = new GeoData(), CountryId = Guid.Empty, GeoDataId = Guid.Empty
        });
        await context.SaveChangesAsync();
        return context;
    }

    private static ImportCountriesCommandHandler CountryHandler(AppDbContext c) =>
        new(new CountryRepository(c), new TabularFileService(), new UnitOfWork(c));

    private static ImportCitiesCommandHandler CityHandler(AppDbContext c) =>
        new(new CityRepository(c), new CountryRepository(c), new TabularFileService(), new UnitOfWork(c));

    private static ImportStadiumsCommandHandler StadiumHandler(AppDbContext c) =>
        new(new StadiumRepository(c), new CityRepository(c), new TabularFileService(), new UnitOfWork(c));

    // ---------- Country ----------

    [Fact]
    public async Task Countries_ValidFile_ShouldSaveAllWithSlugAndPublicId()
    {
        await using var context = NewContext();

        var result = await CountryHandler(context).Handle(
            new ImportCountriesCommand { File = Csv("PublicId,Name\r\n,Croatia\r\nabc,Serbia\r\n") }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Imported.Should().Be(2);
        var saved = await context.Countries.OrderBy(c => c.Name).ToListAsync();
        saved.Select(c => c.Slug).Should().Equal("croatia", "serbia");
        saved.Should().OnlyContain(c => c.PublicId.Length == 32, "the PublicId column in the file is ignored");
    }

    [Fact]
    public async Task Countries_DuplicateNames_ShouldReportRowsAndSaveNothing()
    {
        await using var context = await SeedBosniaAsync();

        var result = await CountryHandler(context).Handle(
            new ImportCountriesCommand { File = Csv("Name\r\nCroatia\r\nBosnia and Herzegovina\r\ncroatia\r\n") }, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Imported.Should().Be(0);
        result.Errors.Select(e => (e.Row, e.Column)).Should().Equal((3, "Name"), (4, "Name"));
        (await context.Countries.CountAsync()).Should().Be(1, "all-or-nothing: Croatia (row 2) is not saved either");
    }

    [Fact]
    public async Task Countries_MissingNameColumn_ShouldReportHeaderError()
    {
        await using var context = NewContext();

        var result = await CountryHandler(context).Handle(
            new ImportCountriesCommand { File = Csv("Title\r\nCroatia\r\n") }, CancellationToken.None);

        result.Errors.Should().ContainSingle(e => e.Row == 1 && e.Column == "Name");
    }

    [Fact]
    public async Task Countries_UnreadableExcel_ShouldReportInsteadOfThrowing()
    {
        await using var context = NewContext();

        var result = await CountryHandler(context).Handle(
            new ImportCountriesCommand { File = Csv("Name\r\nCroatia", "countries.xlsx") }, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Row == 1);
    }

    [Fact]
    public async Task Countries_JsonFile_ShouldBeImported()
    {
        await using var context = NewContext();

        var result = await CountryHandler(context).Handle(
            new ImportCountriesCommand { File = Csv("[{\"Name\": \"Croatia\", \"Slug\": \"x\"}, {\"Name\": \"Serbia\"}]", "countries.json") },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Imported.Should().Be(2);
    }

    // ---------- City ----------

    [Fact]
    public async Task Cities_ValidFile_ShouldSaveCityWithGeoDataAndCountry()
    {
        await using var context = await SeedBosniaAsync();
        const string csv = "Name,Country,Population,IsCapital,Latitude,Longitude,Description\r\n" +
                           "Mostar,bosnia and herzegovina,105797,no,\"43,3438\",17.8078,Old Bridge\r\n";

        var result = await CityHandler(context).Handle(new ImportCitiesCommand { File = Csv(csv) }, CancellationToken.None);

        result.Success.Should().BeTrue(string.Join("; ", result.Errors.Select(e => e.Message)));
        var mostar = await context.Cities.Include(c => c.GeoData).Include(c => c.Country).SingleAsync(c => c.Slug == "mostar");
        mostar.Country!.Name.Should().Be("Bosnia and Herzegovina");
        mostar.Population.Should().Be(105797);
        mostar.IsCapital.Should().BeFalse();
        mostar.GeoData!.Latitude.Should().Be(43.3438m, "a decimal comma is accepted");
        mostar.GeoData.Description.Should().Be("Old Bridge");
    }

    [Fact]
    public async Task Cities_BadRows_ShouldListEveryProblemAndSaveNothing()
    {
        await using var context = await SeedBosniaAsync();
        const string csv = "Name,Country,Population,Latitude,Longitude\r\n" +
                           "Mostar,Bosnia and Herzegovina,105797,43.34,17.80\r\n" +
                           "Zagreb,Croatia,800000,45.81,15.98\r\n" +
                           "Tuzla,Bosnia and Herzegovina,many,44.53,18.67\r\n" +
                           "Bihac,Bosnia and Herzegovina,56000,95,15.87\r\n";

        var result = await CityHandler(context).Handle(new ImportCitiesCommand { File = Csv(csv) }, CancellationToken.None);

        result.Errors.Select(e => (e.Row, e.Column)).Should().Equal((3, "Country"), (4, "Population"), (5, "Latitude"));
        (await context.Cities.CountAsync()).Should().Be(1, "only the seeded Sarajevo, the valid Mostar row is not saved");
    }

    // ---------- Stadium ----------

    [Fact]
    public async Task Stadiums_ValidFile_ShouldResolveCityByCityAndCountry()
    {
        await using var context = await SeedBosniaAsync();
        const string csv = "Name,City,Country,Capacity,DateOfConstruction,Latitude,Longitude,GeoDescription\r\n" +
                           "Grbavica,Sarajevo,Bosnia and Herzegovina,13146,1951-09-16,43.8530,18.3893,Zeljo\r\n";

        var result = await StadiumHandler(context).Handle(new ImportStadiumsCommand { File = Csv(csv) }, CancellationToken.None);

        result.Success.Should().BeTrue(string.Join("; ", result.Errors.Select(e => e.Message)));
        var grbavica = await context.Stadiums.Include(s => s.City).Include(s => s.GeoData).SingleAsync();
        grbavica.City!.Name.Should().Be("Sarajevo");
        grbavica.DateOfConstruction.Should().Be(new DateTime(1951, 9, 16));
        grbavica.GeoData!.Description.Should().Be("Zeljo");
    }

    [Fact]
    public async Task Stadiums_UnknownCityAndBadDate_ShouldBeReported()
    {
        await using var context = await SeedBosniaAsync();
        const string csv = "Name,City,Country,Capacity,DateOfConstruction,Latitude,Longitude,GeoDescription\r\n" +
                           "Tusanj,Tuzla,Bosnia and Herzegovina,7200,1957-01-01,44.54,18.68,North\r\n" +
                           "Kosevo,Sarajevo,Bosnia and Herzegovina,34500,yesterday,43.87,18.41,Olympic\r\n";

        var result = await StadiumHandler(context).Handle(new ImportStadiumsCommand { File = Csv(csv) }, CancellationToken.None);

        result.Errors.Select(e => (e.Row, e.Column)).Should().Equal((2, "City"), (3, "DateOfConstruction"));
        (await context.Stadiums.CountAsync()).Should().Be(0);
    }

    // ---------- Command validator ----------

    [Theory]
    [InlineData("countries.txt", 10)]
    [InlineData("countries.csv", 0)]
    [InlineData("countries.xlsx", 3 * 1024 * 1024)]
    public void ImportValidator_BadFile_ShouldFail(string fileName, long length)
    {
        var file = new Mock<IFormFile>();
        file.Setup(f => f.FileName).Returns(fileName);
        file.Setup(f => f.Length).Returns(length);

        new ImportCountriesCommandValidator().Validate(new ImportCountriesCommand { File = file.Object })
            .IsValid.Should().BeFalse();
    }

    [Fact]
    public void ImportValidator_NoFile_ShouldFail()
    {
        new ImportCountriesCommandValidator().Validate(new ImportCountriesCommand()).IsValid.Should().BeFalse();
    }
}
