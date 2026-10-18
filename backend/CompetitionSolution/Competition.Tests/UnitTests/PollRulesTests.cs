using Competition.Application.Auth.Interfaces;
using Competition.Application.Interfaces;
using Competition.Application.Polls;
using Competition.Domain.Entities.MatchEntities;
using FluentAssertions;
using MediatR;
using Moq;
using NodaTime;
using Match = Competition.Domain.Entities.MatchEntities.Match;

namespace Competition.Tests.UnitTests;

public class PollRulesTests
{
    [Theory]
    [InlineData(true)] [InlineData(false)]
    public async Task VoteWindow_RejectsBeforeOpening_AndExactlyAtClosing(bool scheduled)
    {
        var now = Instant.FromUtc(2026, 10, 18, 12, 0);
        var store = new Mock<IPollStore>(); var unit = new Mock<IUnitOfWork>(); var current = new Mock<ICurrentUser>();
        current.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        var clock = new Mock<IClock>(); clock.Setup(x => x.GetCurrentInstant()).Returns(now);
        store.Setup(x => x.UserIsActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var poll = new Poll { OpensAt = now + Duration.FromHours(scheduled ? 1 : -1), ClosesAt = now + Duration.FromHours(scheduled ? 2 : 0) };
        store.Setup(x => x.FindAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(poll);
        var handler = new VotePollHandler(store.Object, current.Object, unit.Object, Mock.Of<IPublisher>(), clock.Object);
        var act = () => handler.Handle(new VotePollCommand { PollPublicId = new string('a', 32), OptionPublicId = new string('b', 32) }, default);
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        unit.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task FailedSave_DoesNotPublishPollResults()
    {
        var now = SystemClock.Instance.GetCurrentInstant(); var store = new Mock<IPollStore>(); var lookup = new Mock<IPublicIdLookup>();
        var unit = new Mock<IUnitOfWork>(); var publisher = new Mock<IPublisher>();
        lookup.Setup(x => x.GetRequiredIdAsync<Match>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());
        store.Setup(x => x.AddAsync(It.IsAny<Poll>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        unit.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new IOException("Write failed."));
        var handler = new CreatePollHandler(store.Object, lookup.Object, unit.Object, publisher.Object, SystemClock.Instance);
        var act = () => handler.Handle(new CreatePollCommand { MatchPublicId = new string('a', 32), Question = "Who wins?", Options = ["A", "B"],
            OpensAt = now.ToDateTimeOffset(), ClosesAt = (now + Duration.FromHours(1)).ToDateTimeOffset() }, default);
        await act.Should().ThrowAsync<IOException>(); publisher.VerifyNoOtherCalls();
    }
}
