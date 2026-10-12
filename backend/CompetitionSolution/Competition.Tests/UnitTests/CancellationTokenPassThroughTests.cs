using Competition.Application.Commands.CountryCommands.Create;
using Competition.Application.Commands.StadiumCommands.Create;
using Competition.Application.DTOs.CountryDto;
using Competition.Application.DTOs.StadiumDto;
using Competition.Application.Interfaces;
using Competition.Application.Services;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Competition.Tests.UnitTests;

/// <summary>RS1 rule: the request's CancellationToken must reach the image upload, not <c>default</c>.</summary>
public class CancellationTokenPassThroughTests : BaseUnitTest
{
    private readonly Mock<IImageService> _imageServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CancellationTokenSource _cts = new();

    public CancellationTokenPassThroughTests()
    {
        _imageServiceMock
            .Setup(s => s.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://example.test/images/picture.png");
    }

    [Fact]
    public async Task CreateCountry_ShouldPassRequestTokenToImageUpload()
    {
        // Arrange
        var countryMock = new Mock<ICountry>();
        MapperMock.Setup(m => m.Map<Country>(It.IsAny<CreateCountryCommands>())).Returns(new Country { Name = "Croatia" });
        MapperMock.Setup(m => m.Map<CountryNameDto>(It.IsAny<Country>())).Returns(new CountryNameDto { Name = "Croatia" });
        var handler = new CreateCountryCommandHandler(Mapper, countryMock.Object, _unitOfWorkMock.Object, _imageServiceMock.Object);
        var command = new CreateCountryCommands { Name = "Croatia", Picture = Mock.Of<IFormFile>() };

        // Act
        await handler.Handle(command, _cts.Token);

        // Assert
        _imageServiceMock.Verify(s => s.UploadImageAsync(command.Picture!, _cts.Token), Times.Once);
        countryMock.Verify(r => r.AddAsync(It.IsAny<Country>(), _cts.Token), Times.Once);
    }

    [Fact]
    public async Task CreateStadium_WithPicture_ShouldPassRequestTokenToImageUpload()
    {
        // Arrange
        var stadiumMock = new Mock<IStadium>();
        var geoDataMock = new Mock<IGeoData>();
        MapperMock.Setup(m => m.Map<GeoData>(It.IsAny<CreateStadiumCommand>())).Returns(new GeoData());
        MapperMock.Setup(m => m.Map<Stadium>(It.IsAny<CreateStadiumCommand>())).Returns(new Stadium { Name = "Koševo", Slug = "kosevo" });
        MapperMock.Setup(m => m.Map<StadiumDetailsDto>(It.IsAny<Stadium?>())).Returns(new StadiumDetailsDto { Name = "Koševo" });
        var handler = new CreateStadiumCommandHandler(stadiumMock.Object, geoDataMock.Object, Mapper, _unitOfWorkMock.Object, _imageServiceMock.Object);
        var command = new CreateStadiumCommand { Name = "Koševo", Capacity = 34000, Picture = Mock.Of<IFormFile>() };

        // Act
        var result = await handler.Handle(command, _cts.Token);

        // Assert
        result.Name.Should().Be("Koševo");
        _imageServiceMock.Verify(s => s.UploadImageAsync(command.Picture!, _cts.Token), Times.Once);
        stadiumMock.Verify(r => r.AddAsync(It.IsAny<Stadium>(), _cts.Token), Times.Once);
    }
}
