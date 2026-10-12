using Competition.Application.Commands.CompetitionCommands.Create;
using Competition.Application.Commands.CompetitionCommands.Delete;
using Competition.Application.Commands.RoundCommands.Delete;
using Competition.Application.Commands.SeasonCommands.Create;
using Competition.Application.DTOs.SeasonDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.CompetitionEntities;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using FluentValidation;
using Moq;
using Xunit;
using CompetitionEntity = Competition.Domain.Entities.CompetitionEntities.Competition;

namespace Competition.Tests.UnitTests.Competitions;

public class CompetitionHandlerTests : BaseUnitTest
{
    private readonly Mock<ICompetition> _competitionMock = new();
    private readonly Mock<ISeason> _seasonMock = new();
    private readonly Mock<IRound> _roundMock = new();
    private readonly Mock<ICountry> _countryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task CreateCompetition_DuplicateName_ShouldThrowValidationException()
    {
        // Arrange
        _competitionMock.Setup(r => r.SlugExistsAsync("premier-league", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new CreateCompetitionCommandHandler(_competitionMock.Object, _countryMock.Object, _unitOfWorkMock.Object, Mapper);

        // Act
        var act = () => handler.Handle(new CreateCompetitionCommand { Name = " Premier League " }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>().WithMessage("*already exists*");
        _competitionMock.Verify(r => r.AddAsync(It.IsAny<CompetitionEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCompetition_WithSeasons_ShouldBlockDelete()
    {
        // Arrange
        var competition = new CompetitionEntity { Id = Guid.NewGuid(), Name = "Premier League", Slug = "premier-league" };
        _competitionMock.Setup(r => r.GetByIdAsync(competition.Id, It.IsAny<CancellationToken>())).ReturnsAsync(competition);
        _competitionMock.Setup(r => r.HasSeasonsAsync(competition.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new DeleteCompetitionCommandHandler(_competitionMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => handler.Handle(new DeleteCompetitionCommand { Id = competition.Id }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>().WithMessage("*Delete its seasons first*");
        _competitionMock.Verify(r => r.DeleteAsync(It.IsAny<CompetitionEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteRound_WithoutMatches_ShouldSoftDeleteThroughRepository()
    {
        // Arrange
        var round = new Round { Id = Guid.NewGuid(), Name = "Round 1", Number = 1, Slug = "epl-2025-26-round-1" };
        _roundMock.Setup(r => r.GetByIdAsync(round.Id, It.IsAny<CancellationToken>())).ReturnsAsync(round);
        _roundMock.Setup(r => r.HasMatchesAsync(round.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new DeleteRoundCommandHandler(_roundMock.Object, _unitOfWorkMock.Object);

        // Act
        var result = await handler.Handle(new DeleteRoundCommand { Id = round.Id }, CancellationToken.None);

        // Assert
        result.Should().Be(round.Id);
        _roundMock.Verify(r => r.DeleteAsync(round, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSeason_UnknownCompetition_ShouldThrowKeyNotFound()
    {
        // Arrange
        _competitionMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((CompetitionEntity?)null);
        var handler = new CreateSeasonCommandHandler(_seasonMock.Object, _competitionMock.Object, _unitOfWorkMock.Object, Mapper);

        // Act
        var act = () => handler.Handle(NewSeasonCommand(Guid.NewGuid(), isCurrent: false), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CreateSeason_MarkedCurrent_ShouldUnsetOtherCurrentSeasonsAndBuildSlug()
    {
        // Arrange
        var competition = new CompetitionEntity { Id = Guid.NewGuid(), Name = "Premier League", Slug = "premier-league" };
        _competitionMock.Setup(r => r.GetByIdAsync(competition.Id, It.IsAny<CancellationToken>())).ReturnsAsync(competition);
        Season? added = null;
        _seasonMock.Setup(r => r.AddAsync(It.IsAny<Season>(), It.IsAny<CancellationToken>()))
            .Callback<Season, CancellationToken>((s, _) => added = s);
        MapperMock.Setup(m => m.Map<SeasonDetailsDto>(It.IsAny<Season?>())).Returns(new SeasonDetailsDto { Name = "2025/26" });
        var handler = new CreateSeasonCommandHandler(_seasonMock.Object, _competitionMock.Object, _unitOfWorkMock.Object, Mapper);

        // Act
        var result = await handler.Handle(NewSeasonCommand(competition.Id, isCurrent: true), CancellationToken.None);

        // Assert
        result.Name.Should().Be("2025/26");
        added.Should().NotBeNull();
        added!.Slug.Should().Be("premier-league-2025-26");
        added.IsCurrent.Should().BeTrue();
        _seasonMock.Verify(r => r.UnsetCurrentAsync(competition.Id, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static CreateSeasonCommand NewSeasonCommand(Guid competitionId, bool isCurrent) => new()
    {
        CompetitionId = competitionId,
        Name = "2025/26",
        StartDate = new DateOnly(2025, 8, 1),
        EndDate = new DateOnly(2026, 5, 31),
        IsCurrent = isCurrent
    };
}
