using Competition.Application.Commands.StadiumCommands.Delete;
using Competition.Application.Commands.StadiumCommands.Update;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.StadiumDto;
using Competition.Application.Interfaces;
using Competition.Application.Queries.Stadium;
using Competition.Application.Queries.Stadium.GetAll;
using Competition.Application.Services;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Repositories;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CityEntity = Competition.Domain.Entities.City;
using StadiumEntity = Competition.Domain.Entities.MatchEntities.Stadium;

namespace Competition.Tests.UnitTests.Stadiums;

/// <summary>COMP-71: 5-param backend filter, soft delete and picture cleanup for Stadium.</summary>
public class StadiumFilterTests : BaseUnitTest
{
    private static readonly StadiumListFilter NoFilter = new(null, null, null, null, null);

    private static AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("Stadium_" + Guid.NewGuid()).Options);

    private record Seed(AppDbContext Context, Country Bosnia, CityEntity Sarajevo, CityEntity Zagreb, StadiumEntity Kosevo);

    private static async Task<Seed> SeedAsync()
    {
        var context = NewContext();
        var bosnia = new Country { Name = "Bosnia and Herzegovina", Slug = "bosnia-and-herzegovina" };
        var croatia = new Country { Name = "Croatia", Slug = "croatia" };
        var sarajevo = NewCity("Sarajevo", bosnia);
        var mostar = NewCity("Mostar", bosnia);
        var zagreb = NewCity("Zagreb", croatia);
        var kosevo = NewStadium("Asim Ferhatović Hase", sarajevo, 34_500);
        context.Stadiums.AddRange(
            kosevo,
            NewStadium("Grbavica", sarajevo, 13_000),
            NewStadium("Bijeli Brijeg", mostar, 9_000),
            NewStadium("Maksimir", zagreb, 35_000));
        await context.SaveChangesAsync();
        return new Seed(context, bosnia, sarajevo, zagreb, kosevo);
    }

    private static CityEntity NewCity(string name, Country country) => new()
    {
        Name = name,
        Slug = name.ToLower(),
        Country = country,
        GeoData = new GeoData(),
        CountryId = Guid.Empty,
        GeoDataId = Guid.Empty
    };

    private static StadiumEntity NewStadium(string name, CityEntity city, int capacity) => new()
    {
        Name = name,
        Slug = name.ToLower().Replace(' ', '-'),
        Capacity = capacity,
        City = city,
        GeoData = new GeoData()
    };

    // ---------- Repository ----------

    [Fact]
    public async Task NoFilter_ShouldReturnAllSortedByNameWithCityAndCountry()
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;

        var result = await new StadiumRepository(seed.Context).GetPagedFilteredAsync(NoFilter, 1, 10);

        result.TotalCount.Should().Be(4);
        result.Items.Select(s => s.Name).Should().ContainInOrder("Asim Ferhatović Hase", "Bijeli Brijeg", "Grbavica", "Maksimir");
        result.Items.Should().OnlyContain(s => s.City != null && s.City.Country != null);
    }

    [Fact]
    public async Task Name_ShouldMatchCaseInsensitivePartial()
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;

        var result = await new StadiumRepository(seed.Context).GetPagedFilteredAsync(NoFilter with { Name = "  GRBAV " }, 1, 10);

        result.Items.Should().ContainSingle(s => s.Name == "Grbavica");
    }

    [Fact]
    public async Task CountryAndCity_ShouldFilterByLocation()
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;
        var repository = new StadiumRepository(seed.Context);

        (await repository.GetPagedFilteredAsync(NoFilter with { CountryId = seed.Bosnia.Id }, 1, 10))
            .TotalCount.Should().Be(3);
        (await repository.GetPagedFilteredAsync(NoFilter with { CityId = seed.Sarajevo.Id }, 1, 10))
            .Items.Select(s => s.Name).Should().BeEquivalentTo("Asim Ferhatović Hase", "Grbavica");
        (await repository.GetPagedFilteredAsync(NoFilter with { CountryId = seed.Bosnia.Id, CityId = seed.Zagreb.Id }, 1, 10))
            .TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task CapacityRange_ShouldBeInclusive()
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;

        var result = await new StadiumRepository(seed.Context)
            .GetPagedFilteredAsync(NoFilter with { MinCapacity = 13_000, MaxCapacity = 34_500 }, 1, 10);

        result.Items.Select(s => s.Name).Should().BeEquivalentTo("Asim Ferhatović Hase", "Grbavica");
    }

    [Fact]
    public async Task Paging_ShouldApplyAfterFilter()
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;

        var result = await new StadiumRepository(seed.Context)
            .GetPagedFilteredAsync(NoFilter with { CountryId = seed.Bosnia.Id }, 2, 2);

        result.TotalCount.Should().Be(3);
        result.Items.Should().ContainSingle(s => s.Name == "Grbavica");
    }

    [Fact]
    public async Task DeleteHandler_ShouldHideStadiumFromListAndSlugAndSoftDeleteGeoData()
    {
        var seed = await SeedAsync();
        await using var _ = seed.Context;
        var repository = new StadiumRepository(seed.Context);
        var handler = new DeleteStadiumCommandHandler(repository, new GeoDataRepository(seed.Context), new UnitOfWork(seed.Context));

        await handler.Handle(new DeleteStadiumCommand { Id = seed.Kosevo.Id }, CancellationToken.None);

        (await repository.GetPagedFilteredAsync(NoFilter, 1, 10)).Items.Should().NotContain(s => s.Id == seed.Kosevo.Id);
        (await repository.GetBySlugAsync(seed.Kosevo.Slug!)).Should().BeNull();
        (await seed.Context.GeoDatas.IgnoreQueryFilters().SingleAsync(g => g.Id == seed.Kosevo.GeoDataId))
            .DeletedAt.Should().NotBeNull();
        (await seed.Context.Stadiums.IgnoreQueryFilters().AnyAsync(s => s.Id == seed.Kosevo.Id))
            .Should().BeTrue("soft delete keeps the row");
    }

    // ---------- Validator ----------

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void Validator_InvalidPaging_ShouldFail(int page, int pageSize)
    {
        new GetAllStadiumsQueryValidator().Validate(new GetAllStadiumsQuery { Page = page, PageSize = pageSize })
            .IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_AllFiltersSet_ShouldPass()
    {
        var result = new GetAllStadiumsQueryValidator().Validate(new GetAllStadiumsQuery
        {
            Name = "Koševo", CountryId = Guid.NewGuid(), CityId = Guid.NewGuid(), MinCapacity = 0, MaxCapacity = 50_000
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_InvalidFilters_ShouldFail()
    {
        var validator = new GetAllStadiumsQueryValidator();

        validator.Validate(new GetAllStadiumsQuery { MinCapacity = 20_000, MaxCapacity = 10_000 }).IsValid.Should().BeFalse();
        validator.Validate(new GetAllStadiumsQuery { MinCapacity = -1 }).IsValid.Should().BeFalse();
        validator.Validate(new GetAllStadiumsQuery { CityId = Guid.Empty }).IsValid.Should().BeFalse();
        validator.Validate(new GetAllStadiumsQuery { CountryId = Guid.Empty }).IsValid.Should().BeFalse();
        validator.Validate(new GetAllStadiumsQuery { Name = new string('a', 151) }).IsValid.Should().BeFalse();
    }

    // ---------- Picture cleanup ----------

    private (UpdateStadiumCommandHandler Handler, Mock<IImageService> Images) NewUpdateHandler(StadiumEntity stadium)
    {
        var stadiums = new Mock<IStadium>();
        stadiums.Setup(s => s.GetBySlugAsync(stadium.Slug!, It.IsAny<CancellationToken>())).ReturnsAsync(stadium);
        MapperMock.Setup(m => m.Map<StadiumDetailsDto>(It.IsAny<StadiumEntity>())).Returns(new StadiumDetailsDto());
        var images = new Mock<IImageService>();
        images.Setup(i => i.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>())).ReturnsAsync("https://blob/new.png");
        var handler = new UpdateStadiumCommandHandler(Mapper, stadiums.Object, Mock.Of<IGeoData>(), Mock.Of<IUnitOfWork>(), images.Object);
        return (handler, images);
    }

    private static UpdateStadiumCommand UpdateCommand(IFormFile? picture) => new()
    {
        Slug = "grbavica", Name = "Grbavica", Capacity = 13_000, CityId = Guid.NewGuid(), Picture = picture
    };

    [Fact]
    public async Task Update_NewPicture_ShouldDeleteOldPicture()
    {
        var stadium = new StadiumEntity { Name = "Grbavica", Slug = "grbavica", PictureUrl = "https://blob/old.png" };
        var (handler, images) = NewUpdateHandler(stadium);

        await handler.Handle(UpdateCommand(Mock.Of<IFormFile>()), CancellationToken.None);

        stadium.PictureUrl.Should().Be("https://blob/new.png");
        images.Verify(i => i.DeleteImageAsync("https://blob/old.png", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_NoNewPicture_ShouldKeepOldPicture()
    {
        var stadium = new StadiumEntity { Name = "Grbavica", Slug = "grbavica", PictureUrl = "https://blob/old.png" };
        var (handler, images) = NewUpdateHandler(stadium);

        await handler.Handle(UpdateCommand(null), CancellationToken.None);

        stadium.PictureUrl.Should().Be("https://blob/old.png");
        images.Verify(i => i.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_OldPictureDeleteFails_ShouldStillSucceed()
    {
        var stadium = new StadiumEntity { Name = "Grbavica", Slug = "grbavica", PictureUrl = "https://blob/old.png" };
        var (handler, images) = NewUpdateHandler(stadium);
        images.Setup(i => i.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new IOException("blob down"));

        var act = () => handler.Handle(UpdateCommand(Mock.Of<IFormFile>()), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
