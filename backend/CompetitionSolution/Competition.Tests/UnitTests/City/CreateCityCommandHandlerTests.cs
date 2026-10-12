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
    private readonly CreateCityCommandHandler _handler;

    public CreateCityCommandHandlerTests()
    {
        _cityRepositoryMock = new Mock<ICity>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        MapperMock.Setup(m => m.Map<GeoData>(It.IsAny<CreateCityCommand>())).Returns(new GeoData());
        MapperMock.Setup(m => m.Map<Competition.Domain.Entities.City>(It.IsAny<CreateCityCommand>()))
            .Returns(new Competition.Domain.Entities.City { Name = "Sarajevo", Slug = "sarajevo", GeoDataId = Guid.NewGuid(), CountryId = Guid.NewGuid() });
        MapperMock.Setup(m => m.Map<CityNameDto>(It.IsAny<Competition.Domain.Entities.City>()))
            .Returns(new CityNameDto { Name = "Sarajevo" });

        _handler = new CreateCityCommandHandler(
            _cityRepositoryMock.Object,
            Mapper,
            _unitOfWorkMock.Object);
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
            CountryId = Guid.NewGuid(),
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
}
