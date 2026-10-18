using Competition.Api.Live;
using Competition.Application.Live;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NodaTime;

namespace Competition.Tests.UnitTests;

public class MatchBroadcastTests
{
    [Fact]
    public async Task FailedSocketSend_DoesNotFailAnAlreadyCommittedWrite()
    {
        var proxy = new Mock<IClientProxy>();
        proxy.Setup(x => x.SendCoreAsync("MatchChanged", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Transport unavailable."));
        var clients = new Mock<IHubClients>(); clients.Setup(x => x.Group(It.IsAny<string>())).Returns(proxy.Object);
        var hub = new Mock<IHubContext<MatchHub>>(); hub.SetupGet(x => x.Clients).Returns(clients.Object);
        var handler = new MatchChangedHandler(hub.Object, SystemClock.Instance, NullLogger<MatchChangedHandler>.Instance);
        await handler.Handle(new MatchChangedNotification(new string('a', 32), "events"), CancellationToken.None);
        proxy.Verify(x => x.SendCoreAsync("MatchChanged", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
