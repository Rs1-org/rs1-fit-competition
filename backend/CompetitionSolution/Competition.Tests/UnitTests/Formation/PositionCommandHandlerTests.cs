using Competition.Application.Commands.PositionCommands.Create;
using Competition.Application.Commands.SubPositionCommands.Create;
using Competition.Application.Helpers;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.FormationEntities;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Moq;
using Xunit;

namespace Competition.Tests.UnitTests.FormationTests;

public class CreatePositionCommandHandlerTests : BaseUnitTest
{
    private readonly Mock<IPosition> _positions = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Handle_NewName_CreatesPosition()
    {
        _positions.Setup(x => x.GetBySlugAsync("defender", It.IsAny<CancellationToken>())).ReturnsAsync((Position?)null);
        var handler = new CreatePositionCommandHandler(_positions.Object, _unitOfWork.Object);

        var result = await handler.Handle(new CreatePositionCommand { Name = "Defender" }, CancellationToken.None);

        result.Name.Should().Be("Defender");
        result.Slug.Should().Be("defender");
        _positions.Verify(x => x.AddAsync(It.Is<Position>(p => p.Name == "Defender" && p.Slug == "defender"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateSlug_Throws()
    {
        _positions.Setup(x => x.GetBySlugAsync("defender", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Position { Name = "Defender", Slug = "defender" });
        var handler = new CreatePositionCommandHandler(_positions.Object, _unitOfWork.Object);

        var act = () => handler.Handle(new CreatePositionCommand { Name = "Defender" }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}

public class CreateSubPositionCommandHandlerTests : BaseUnitTest
{
    [Fact]
    public async Task Handle_UnknownPosition_Throws()
    {
        var positions = new Mock<IPosition>();
        var subPositions = new Mock<ISubPosition>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var positionId = Guid.NewGuid();
        positions.Setup(x => x.GetByIdAsync(positionId, It.IsAny<CancellationToken>())).ReturnsAsync((Position?)null);
        var handler = new CreateSubPositionCommandHandler(subPositions.Object, positions.Object, unitOfWork.Object);

        var act = () => handler.Handle(new CreateSubPositionCommand { Name = "Left back", PositionId = positionId }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ValidPosition_SlugsWithParentName()
    {
        var position = new Position { Id = Guid.NewGuid(), Name = "Defender", Slug = "defender" };
        var positions = new Mock<IPosition>();
        var subPositions = new Mock<ISubPosition>();
        var unitOfWork = new Mock<IUnitOfWork>();
        positions.Setup(x => x.GetByIdAsync(position.Id, It.IsAny<CancellationToken>())).ReturnsAsync(position);
        subPositions.Setup(x => x.GetBySlugAsync("defender-left-back", It.IsAny<CancellationToken>())).ReturnsAsync((SubPosition?)null);
        var handler = new CreateSubPositionCommandHandler(subPositions.Object, positions.Object, unitOfWork.Object);

        var result = await handler.Handle(new CreateSubPositionCommand { Name = "Left back", PositionId = position.Id }, CancellationToken.None);

        result.Slug.Should().Be("defender-left-back");
        result.PositionId.Should().Be(position.Id);
        result.PositionName.Should().Be("Defender");
    }
}

public class PlayerPositionRulesTests
{
    [Fact]
    public void EnsureConsistent_SubPositionFromAnotherPosition_Throws()
    {
        var position = new Position { Id = Guid.NewGuid(), Name = "Defender" };
        var other = new Position { Id = Guid.NewGuid(), Name = "Midfielder" };
        var sub = new SubPosition { Id = Guid.NewGuid(), Name = "Left back", PositionId = other.Id, Position = other };

        var act = () => PlayerPositionRules.EnsureConsistent(position.Id, position, sub.Id, sub);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void EnsureConsistent_MatchingParent_AllowsAssignment()
    {
        var position = new Position { Id = Guid.NewGuid(), Name = "Defender" };
        var sub = new SubPosition { Id = Guid.NewGuid(), Name = "Left back", PositionId = position.Id, Position = position };

        var act = () => PlayerPositionRules.EnsureConsistent(position.Id, position, sub.Id, sub);

        act.Should().NotThrow();
    }
}
