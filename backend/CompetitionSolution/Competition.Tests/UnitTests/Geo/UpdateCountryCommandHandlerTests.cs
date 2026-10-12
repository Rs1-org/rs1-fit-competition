using Competition.Application.Commands.CountryCommands.Update;
using Competition.Application.DTOs.CountryDto;
using Competition.Application.Interfaces;
using Competition.Application.Services;
using Competition.Domain.Entities;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Competition.Tests.UnitTests.Geo;

/// <summary>COMP-77/COMP-121: UpdateCountryCommandHandler slug regeneration, safe picture replacement and single save.</summary>
public class UpdateCountryCommandHandlerTests : BaseUnitTest
{
    private readonly Mock<ICountry> _countries = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IImageService> _images = new();

    private UpdateCountryCommandHandler NewHandler(Country country)
    {
        _countries.Setup(c => c.GetBySlugAsync(country.Slug!, It.IsAny<CancellationToken>())).ReturnsAsync(country);
        MapperMock.Setup(m => m.Map<CountryNameDto>(It.IsAny<Country>()))
            .Returns((object source) => new CountryNameDto { Name = ((Country)source).Name });
        _images.Setup(i => i.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>())).ReturnsAsync("https://blob/new.png");
        return new UpdateCountryCommandHandler(Mapper, _countries.Object, _unitOfWork.Object, _images.Object, NullLogger<UpdateCountryCommandHandler>.Instance);
    }

    private static Country Bosnia() => new()
    {
        Name = "Bosnia and Herzegovina",
        Slug = "bosnia-and-herzegovina",
        PictureUrl = "https://blob/old.png"
    };

    [Fact]
    public async Task NameChange_ShouldRegenerateSlugWithoutDiacritics()
    {
        var country = Bosnia();
        var handler = NewHandler(country);

        var result = await handler.Handle(
            new UpdateCountryCommands { Slug = "bosnia-and-herzegovina", Name = "Bosna i Hercegovina Đ Ž" },
            CancellationToken.None);

        country.Name.Should().Be("Bosna i Hercegovina Đ Ž");
        country.Slug.Should().Be("bosna-i-hercegovina-d-z");
        result.Name.Should().Be("Bosna i Hercegovina Đ Ž");
    }

    [Fact]
    public async Task UnknownSlug_ShouldThrowAndNotSave()
    {
        var handler = NewHandler(Bosnia());

        var act = () => handler.Handle(new UpdateCountryCommands { Slug = "atlantis", Name = "Atlantis" }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task NewPicture_ShouldDeleteOldAndStoreNewUrl()
    {
        var country = Bosnia();
        var handler = NewHandler(country);

        await handler.Handle(
            new UpdateCountryCommands { Slug = country.Slug!, Name = country.Name, Picture = Mock.Of<IFormFile>() },
            CancellationToken.None);

        country.PictureUrl.Should().Be("https://blob/new.png");
        _images.Verify(i => i.DeleteImageAsync("https://blob/old.png", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NewPicture_OldPictureDeleteFails_ShouldStillSucceed()
    {
        var country = Bosnia();
        var handler = NewHandler(country);
        _images.Setup(i => i.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new IOException("blob down"));

        var act = () => handler.Handle(
            new UpdateCountryCommands { Slug = country.Slug!, Name = country.Name, Picture = Mock.Of<IFormFile>() },
            CancellationToken.None);

        await act.Should().NotThrowAsync();
        country.PictureUrl.Should().Be("https://blob/new.png");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NewPicture_SaveFails_ShouldKeepOldPictureAndRemoveUploadedOne()
    {
        var country = Bosnia();
        var handler = NewHandler(country);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("db down"));

        var act = () => handler.Handle(
            new UpdateCountryCommands { Slug = country.Slug!, Name = country.Name, Picture = Mock.Of<IFormFile>() },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _images.Verify(i => i.DeleteImageAsync("https://blob/old.png", It.IsAny<CancellationToken>()), Times.Never);
        _images.Verify(i => i.DeleteImageAsync("https://blob/new.png", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NewPicture_SaveFailsAndCleanupFails_ShouldStillReportSaveError()
    {
        var country = Bosnia();
        var handler = NewHandler(country);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("db down"));
        _images.Setup(i => i.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new IOException("blob down"));

        var act = () => handler.Handle(
            new UpdateCountryCommands { Slug = country.Slug!, Name = country.Name, Picture = Mock.Of<IFormFile>() },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("db down");
    }

    [Fact]
    public async Task NewPicture_CountryWithoutPicture_ShouldNotDeleteAnything()
    {
        var country = Bosnia();
        country.PictureUrl = null;
        var handler = NewHandler(country);

        await handler.Handle(
            new UpdateCountryCommands { Slug = country.Slug!, Name = country.Name, Picture = Mock.Of<IFormFile>() },
            CancellationToken.None);

        country.PictureUrl.Should().Be("https://blob/new.png");
        _images.Verify(i => i.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task NoPicture_ShouldKeepOldUrlAndNotTouchImageStorage()
    {
        var country = Bosnia();
        var handler = NewHandler(country);

        await handler.Handle(new UpdateCountryCommands { Slug = country.Slug!, Name = "Bosnia" }, CancellationToken.None);

        country.PictureUrl.Should().Be("https://blob/old.png");
        _images.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_ShouldSaveOnceAndPassCancellationToken()
    {
        var country = Bosnia();
        var handler = NewHandler(country);
        using var cts = new CancellationTokenSource();

        await handler.Handle(new UpdateCountryCommands { Slug = country.Slug!, Name = "Bosnia" }, cts.Token);

        _countries.Verify(c => c.GetBySlugAsync("bosnia-and-herzegovina", cts.Token), Times.Once);
        _countries.Verify(c => c.UpdateAsync(country, cts.Token), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
    }
}
