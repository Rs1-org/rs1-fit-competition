using Competition.Application.Common.Validation;
using Competition.Application.Interfaces;
using Competition.Domain.Entities.MatchEntities;
using Microsoft.AspNetCore.SignalR;

namespace Competition.Api.Live;

/// <summary>Public, read-only subscriptions. All mutations remain on the authorized CQRS endpoints.</summary>
public class MatchHub(IPublicIdLookup lookup) : Hub
{
    private const string RoomKey = "match-room";
    public static string Room(string publicId) => $"match:{publicId}";

    public async Task WatchMatch(string matchPublicId)
    {
        if (!PublicIdRules.IsValidFormat(matchPublicId)) throw new HubException("Choose a valid match public ID.");
        if (await lookup.GetIdByPublicIdAsync<Match>(matchPublicId, Context.ConnectionAborted) is null)
            throw new HubException("Match not found.");
        var room = Room(matchPublicId);
        if (Context.Items.TryGetValue(RoomKey, out var previous) && previous is string old && old != room)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, old, Context.ConnectionAborted);
        await Groups.AddToGroupAsync(Context.ConnectionId, room, Context.ConnectionAborted);
        Context.Items[RoomKey] = room;
    }
    public async Task UnwatchMatch()
    {
        if (Context.Items.Remove(RoomKey, out var previous) && previous is string room)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room, Context.ConnectionAborted);
    }
}
