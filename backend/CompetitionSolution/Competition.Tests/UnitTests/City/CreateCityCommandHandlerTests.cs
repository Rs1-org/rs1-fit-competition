using Competition.Application.Commands.CityCommands.Create;
using Competition.Application.DTOs.CityDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Moq;
using Xunit;

namespace Competition.Tests.UnitTests.City;

public class CreateCityCommandHandlerTests : BaseUnitTest
{
    private readonly Mock<ICity> _cityRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPublicIdLookup> _lookupMock;
    private readonly Guid _countryId = Guid.NewGuid();
    private const string CountryPublicId = "0f1e2d3c4b5a69788796a5b4c3d2e1f0";
    private readonly CreateCityCommandHandler _handler;

    public CreateCityCommandHandlerTests()
    {
        _cityRepositoryMock = new Mock<ICity>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _lookupMock = new Mock<IPublicIdLookup>();
        _lookupMock.Setup(l => l.GetRequiredIdAsync<Competition.Domain.Entities.Country>(CountryPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_countryId);

        MapperMock.Setup(m => m.Map<GeoData>(It.IsAny<CreateCityCommand>())).Returns(new GeoData());
        MapperMock.Setup(m => m.Map<Competition.Domain.Entities.City>(It.IsAny<CreateCityCommand>()))
            .Returns(new Competition.Domain.Entities.City { Name = "Sarajevo", Slug = "sarajevo", GeoDataId = Guid.NewGuid(), CountryId = Guid.NewGuid() });
        MapperMock.Setup(m => m.Map<CityNameDto>(It.IsAny<Competition.Domain.Entities.City>()))
            .Returns(new CityNameDto { Name = "Sarajevo" });

        _handler = new CreateCityCommandHandler(
            _cityRepositoryMock.Object,
            Mapper,
            _unitOfWorkMock.Object,
            _lookupMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateCityAndReturnDto()
    {
        // Arrange
        var command = new CreateCityCommand
        {
            Name = "Sarajevo",
            Population = 300000,
            IsCapital = true,
            CountryPublicId = CountryPublicId,
            Latitude = 43.8563m,
            Longitude = 18.4131m,
            Description = "Capital of Bosnia"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Sarajevo");

        // City and its GeoData are added together and saved once (atomic)
        _cityRepositoryMock.Verify(x => x.AddAsync(It.Is<Competition.Domain.Entities.City>(c => c.GeoData != null), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseInternalIdOfTheCountryPublicId()
    {
        // Arrange
        Competition.Domain.Entities.City? added = null;
        _cityRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Competition.Domain.Entities.City>(), It.IsAny<CancellationToken>()))
            .Callback<Competition.Domain.Entities.City, CancellationToken>((c, _) => added = c);

        // Act
        await _handler.Handle(new CreateCityCommand { Name = "Mostar", Population = 100000, CountryPublicId = CountryPublicId }, CancellationToken.None);

        // Assert
        added!.CountryId.Should().Be(_countryId);
    }

    [Fact]
    public async Task Handle_UnknownCountryPublicId_ShouldThrow404BeforeSavingAnything()
    {
        // Arrange
        _lookupMock.Setup(l => l.GetRequiredIdAsync<Competition.Domain.Entities.Country>("ffffffffffffffffffffffffffffffff", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        var act = () => _handler.Handle(new CreateCityCommand { Name = "Nowhere", Population = 1, CountryPublicId = "ffffffffffffffffffffffffffffffff" }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
        _cityRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Competition.Domain.Entities.City>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
