using Competition.Application.Commands.FormationCommands.Create;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.FormationEntities;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Moq;
using Xunit;

namespace Competition.Tests.UnitTests.FormationTests;

public class CreateFormationCommandHandlerTests : BaseUnitTest
{
    private readonly Mock<IFormation> _formations = new();
    private readonly Mock<IFormationMark> _marks = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IPublicIdLookup> _lookup = new();
    private readonly CreateFormationCommandHandler _handler;

    public CreateFormationCommandHandlerTests()
    {
        _handler = new CreateFormationCommandHandler(_formations.Object, _marks.Object, _unitOfWork.Object, _lookup.Object);
    }

    [Fact]
    public async Task Handle_MatchingMark_CreatesFormation()
    {
        var mark = new FormationMark { Id = Guid.NewGuid(), PublicId = Guid.NewGuid().ToString("N"), Defenders = 4, Midfielders = 3, Attackers = 3, Slug = "4-3-3" };
        _lookup.Setup(x => x.GetRequiredIdAsync<FormationMark>(mark.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(mark.Id);
        _marks.Setup(x => x.GetByIdAsync(mark.Id, It.IsAny<CancellationToken>())).ReturnsAsync(mark);
        _formations.Setup(x => x.GetByMarkIdAsync(mark.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Formation?)null);
        _formations.Setup(x => x.GetBySlugAsync("4-3-3", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Formation?)null);
        _formations.Setup(x => x.AddAsync(It.IsAny<Formation>(), It.IsAny<CancellationToken>()))
            .Callback<Formation, CancellationToken>((entity, _) =>
            {
                entity.FormationMark = mark;
                _formations.Setup(x => x.GetBySlugAsync("4-3-3", It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            })
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(new CreateFormationCommand { Code = "4-3-3", FormationMarkPublicId = mark.PublicId }, CancellationToken.None);

        result.Code.Should().Be("4-3-3");
        result.Defenders.Should().Be(4);
        result.FormationMarkPublicId.Should().Be(mark.PublicId);
        _formations.Verify(x => x.AddAsync(It.IsAny<Formation>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CodeDoesNotMatchMark_Throws()
    {
        var mark = new FormationMark { Id = Guid.NewGuid(), PublicId = Guid.NewGuid().ToString("N"), Defenders = 4, Midfielders = 4, Attackers = 2 };
        _lookup.Setup(x => x.GetRequiredIdAsync<FormationMark>(mark.PublicId, It.IsAny<CancellationToken>())).ReturnsAsync(mark.Id);
        _marks.Setup(x => x.GetByIdAsync(mark.Id, It.IsAny<CancellationToken>())).ReturnsAsync(mark);

        var act = () => _handler.Handle(new CreateFormationCommand { Code = "4-3-3", FormationMarkPublicId = mark.PublicId }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _formations.Verify(x => x.AddAsync(It.IsAny<Formation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MissingMark_Throws()
    {
        var markId = Guid.NewGuid();
        var publicId = Guid.NewGuid().ToString("N");
        _lookup.Setup(x => x.GetRequiredIdAsync<FormationMark>(publicId, It.IsAny<CancellationToken>())).ReturnsAsync(markId);
        _marks.Setup(x => x.GetByIdAsync(markId, It.IsAny<CancellationToken>())).ReturnsAsync((FormationMark?)null);

        var act = () => _handler.Handle(new CreateFormationCommand { Code = "4-3-3", FormationMarkPublicId = publicId }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
