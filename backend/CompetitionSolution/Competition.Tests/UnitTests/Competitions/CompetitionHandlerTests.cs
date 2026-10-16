using Competition.Application.Commands.CompetitionCommands.Create;
using Competition.Application.Commands.CompetitionCommands.Delete;
using Competition.Application.Commands.RoundCommands.Delete;
using Competition.Application.Commands.SeasonCommands.Create;
using Competition.Application.DTOs.SeasonDto;
using Competition.Application.Interfaces;
using Competition.Domain.Entities;
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
    private readonly Mock<IPublicIdLookup> _lookupMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task CreateCompetition_DuplicateName_ShouldThrowValidationException()
    {
        // Arrange
        _competitionMock.Setup(r => r.SlugExistsAsync("premier-league", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new CreateCompetitionCommandHandler(_competitionMock.Object, _lookupMock.Object, _unitOfWorkMock.Object, Mapper);

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
        var competition = new CompetitionEntity { Id = Guid.NewGuid(), PublicId = BaseEntity.NewPublicId(), Name = "Premier League", Slug = "premier-league" };
        _lookupMock.Setup(l => l.GetRequiredEntityAsync<CompetitionEntity>(competition.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(competition);
        _competitionMock.Setup(r => r.HasSeasonsAsync(competition.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new DeleteCompetitionCommandHandler(_competitionMock.Object, _lookupMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => handler.Handle(new DeleteCompetitionCommand { PublicId = competition.PublicId }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>().WithMessage("*Delete its seasons first*");
        _competitionMock.Verify(r => r.DeleteAsync(It.IsAny<CompetitionEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteRound_WithoutMatches_ShouldSoftDeleteAndReturnPublicId()
    {
        // Arrange
        var round = new Round { Id = Guid.NewGuid(), PublicId = BaseEntity.NewPublicId(), Name = "Round 1", Number = 1, Slug = "epl-2025-26-round-1" };
        _lookupMock.Setup(l => l.GetRequiredEntityAsync<Round>(round.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(round);
        _roundMock.Setup(r => r.HasMatchesAsync(round.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new DeleteRoundCommandHandler(_roundMock.Object, _lookupMock.Object, _unitOfWorkMock.Object);

        // Act
        var result = await handler.Handle(new DeleteRoundCommand { PublicId = round.PublicId }, CancellationToken.None);

        // Assert: the response carries the PublicId, never the internal Guid
        result.Should().Be(round.PublicId);
        _roundMock.Verify(r => r.DeleteAsync(round, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSeason_UnknownCompetitionPublicId_ShouldThrowKeyNotFound()
    {
        // Arrange
        _lookupMock.Setup(l => l.GetRequiredEntityAsync<CompetitionEntity>(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());
        var handler = new CreateSeasonCommandHandler(_seasonMock.Object, _lookupMock.Object, _unitOfWorkMock.Object, Mapper);

        // Act
        var act = () => handler.Handle(NewSeasonCommand(BaseEntity.NewPublicId(), isCurrent: false), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
        _seasonMock.Verify(r => r.AddAsync(It.IsAny<Season>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateSeason_MarkedCurrent_ShouldUnsetOtherCurrentSeasonsAndBuildSlug()
    {
        // Arrange
        var competition = new CompetitionEntity { Id = Guid.NewGuid(), PublicId = BaseEntity.NewPublicId(), Name = "Premier League", Slug = "premier-league" };
        _lookupMock.Setup(l => l.GetRequiredEntityAsync<CompetitionEntity>(competition.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(competition);
        Season? added = null;
        _seasonMock.Setup(r => r.AddAsync(It.IsAny<Season>(), It.IsAny<CancellationToken>()))
            .Callback<Season, CancellationToken>((s, _) => added = s);
        MapperMock.Setup(m => m.Map<SeasonDetailsDto>(It.IsAny<Season?>())).Returns(new SeasonDetailsDto { Name = "2025/26" });
        var handler = new CreateSeasonCommandHandler(_seasonMock.Object, _lookupMock.Object, _unitOfWorkMock.Object, Mapper);

        // Act
        var result = await handler.Handle(NewSeasonCommand(competition.PublicId, isCurrent: true), CancellationToken.None);

        // Assert
        result.Name.Should().Be("2025/26");
        added.Should().NotBeNull();
        added!.CompetitionId.Should().Be(competition.Id);
        added.Slug.Should().Be("premier-league-2025-26");
        added.IsCurrent.Should().BeTrue();
        _seasonMock.Verify(r => r.UnsetCurrentAsync(competition.Id, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static CreateSeasonCommand NewSeasonCommand(string competitionPublicId, bool isCurrent) => new()
    {
        CompetitionPublicId = competitionPublicId,
        Name = "2025/26",
        StartDate = new DateOnly(2025, 8, 1),
        EndDate = new DateOnly(2026, 5, 31),
        IsCurrent = isCurrent
    };
}
