using Competition.Application.Live;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using NodaTime;

namespace Competition.Api.Live;

public record MatchChangeNotice(string MatchPublicId, string Change, DateTimeOffset ChangedAt);

public class MatchChangedHandler(IHubContext<MatchHub> hub, IClock clock, ILogger<MatchChangedHandler> logger)
    : INotificationHandler<MatchChangedNotification>
{
    public async Task Handle(MatchChangedNotification notification, CancellationToken cancellationToken)
    {
        // A socket failure must not turn an already committed write into an apparent failure.
        // Clients fetch the authoritative database snapshot on every successful join/rejoin.
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(2));
            await hub.Clients.Group(MatchHub.Room(notification.MatchPublicId)).SendAsync("MatchChanged",
                new MatchChangeNotice(notification.MatchPublicId, notification.Change, clock.GetCurrentInstant().ToDateTimeOffset()),
                timeout.Token);
        }
        catch (Exception error)
        {
            logger.LogWarning(error, "A committed match change could not be broadcast. Clients will resynchronize on reconnect.");
        }
    }
}
