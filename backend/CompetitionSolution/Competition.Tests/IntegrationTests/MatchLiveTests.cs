using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Competition.Application.Auth.Interfaces;
using Competition.Application.DTOs.MatchDto;
using Competition.Application.Events;
using Competition.Application.Polls;
using Competition.Application.Common.Models;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Competition.Tests.IntegrationTests;

public class MatchLiveTests(WebApplicationFactory<Program> factory) : BaseIntegrationTest(factory)
{
    private async Task<Match> Match()
    {
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var match = StatisticTestData.NewMatch(); db.Matches.Add(match); await db.SaveChangesAsync(); return match;
    }
    private async Task Moderator()
    {
        using var scope = ServiceProvider.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var email = $"live-{Guid.NewGuid():N}@example.invalid";
        var user = new ApplicationUser { Email = email, UserName = email, FirstName = "Live", LastName = "Test" };
        (await manager.CreateAsync(user)).Succeeded.Should().BeTrue(); await manager.AddToRoleAsync(user, "Moderator");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>().GenerateJwtToken(user, ["Moderator"]));
    }
    private async Task<CreateEventCommand> Event(Match match)
    {
        var lookups = (await Client.GetFromJsonAsync<EventLookupsDto>("/api/events/lookups"))!;
        return new() { MatchPublicId = match.PublicId, SelectionPublicId = match.HomeSelection!.PublicId, Minute = 20,
            EventTypePublicId = lookups.EventTypes.First().PublicId, MatchPartPublicId = lookups.MatchParts.First().PublicId };
    }
    private async Task<WireClient> Connect()
    {
        var socket = await Server.CreateWebSocketClient().ConnectAsync(new Uri("ws://localhost/hubs/matches"), CancellationToken.None);
        var wire = new WireClient(socket); await wire.Send(new { protocol = "json", version = 1 });
        (await wire.Receive()).GetRawText().Should().Be("{}"); return wire;
    }

    [Fact]
    public async Task AnonymousSubscriber_ReceivesCommittedEventCreateUpdateDelete_WithPublicIds()
    {
        var match = await Match(); using var wire = await Connect();
        (await wire.Invoke("WatchMatch", match.PublicId)).TryGetProperty("error", out _).Should().BeFalse();
        await Moderator(); var input = await Event(match);
        var created = await Client.PostAsJsonAsync("/api/events", input); created.StatusCode.Should().Be(HttpStatusCode.OK);
        var saved = (await created.Content.ReadFromJsonAsync<SavedEventDto>())!;
        await wire.AssertNotice(match.PublicId, "events", match.Id);
        input.Minute = 21;
        (await Client.PutAsJsonAsync($"/api/events/{saved.PublicId}", input)).StatusCode.Should().Be(HttpStatusCode.OK);
        await wire.AssertNotice(match.PublicId, "events", match.Id);
        (await Client.GetFromJsonAsync<EventDto>($"/api/events/{saved.PublicId}"))!.Minute.Should().Be(21);
        (await Client.DeleteAsync($"/api/events/{saved.PublicId}")).StatusCode.Should().Be(HttpStatusCode.OK);
        await wire.AssertNotice(match.PublicId, "events", match.Id);
    }

    [Fact]
    public async Task ScoreUpdate_Broadcasts_AndSnapshotReturnsCurrentScore()
    {
        var match = await Match(); using var wire = await Connect(); await wire.Invoke("WatchMatch", match.PublicId);
        await Moderator();
        var response = await Client.PutAsJsonAsync($"/api/Match/update/{match.Slug}", new
        {
            date = match.Date.ToDateTimeUtc(), homeScore = 2, awayScore = 1,
            statusPublicId = match.Status!.PublicId, stadiumPublicId = match.Stadium!.PublicId,
            homeSelectionPublicId = match.HomeSelection!.PublicId, awaySelectionPublicId = match.AwaySelection!.PublicId,
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        await wire.AssertNotice(match.PublicId, "match", match.Id);
        var current = (await Client.GetFromJsonAsync<MatchDetailsDto>($"/api/Match/getByPublicId/{match.PublicId}"))!;
        current.HomeScore.Should().Be(2); current.AwayScore.Should().Be(1);
    }

    [Fact]
    public async Task SwitchingSubscription_LeavesOldRoom_AndHubHasNoClientWriteMethod()
    {
        var first = await Match(); var second = await Match(); using var wire = await Connect();
        await wire.Invoke("WatchMatch", first.PublicId); await wire.Invoke("WatchMatch", second.PublicId);
        (await wire.Invoke("Publish", first.PublicId)).TryGetProperty("error", out _).Should().BeTrue();
        await Moderator();
        (await Client.PostAsJsonAsync("/api/events", await Event(first))).StatusCode.Should().Be(HttpStatusCode.OK);
        (await Client.PostAsJsonAsync("/api/events", await Event(second))).StatusCode.Should().Be(HttpStatusCode.OK);
        // If the old room leaked, its earlier notification would arrive first and fail this assertion.
        await wire.AssertNotice(second.PublicId, "events", second.Id);
    }

    [Fact]
    public async Task HubRejectsMalformedOrMissingMatches_AndUnapprovedBrowserOrigins()
    {
        using var wire = await Connect();
        (await wire.Invoke("WatchMatch", "invalid")).TryGetProperty("error", out _).Should().BeTrue();
        (await wire.Invoke("WatchMatch", new string('f', 32))).TryGetProperty("error", out _).Should().BeTrue();
        using var rejected = new HttpRequestMessage(HttpMethod.Post, "/hubs/matches/negotiate?negotiateVersion=1");
        rejected.Headers.Add("Origin", "https://unapproved.example.invalid");
        (await Client.SendAsync(rejected)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var approved = new HttpRequestMessage(HttpMethod.Post, "/hubs/matches/negotiate?negotiateVersion=1");
        approved.Headers.Add("Origin", "http://localhost:4200");
        var result = await Client.SendAsync(approved); result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Headers.GetValues("Access-Control-Allow-Origin").Should().ContainSingle().Which.Should().Be("http://localhost:4200");
    }

    [Fact]
    public async Task PollCreationAndVoting_NotifyEveryMatchSpectator()
    {
        var match = await Match(); using var first = await Connect(); using var second = await Connect();
        await first.Invoke("WatchMatch", match.PublicId); await second.Invoke("WatchMatch", match.PublicId);
        await Moderator();
        var created = await Client.PostAsJsonAsync($"/api/polls/match/{match.PublicId}", new
        { question = "Who wins?", options = new[] { "Home", "Away" }, opensAt = DateTimeOffset.UtcNow.AddMinutes(-1), closesAt = DateTimeOffset.UtcNow.AddHours(1) });
        created.StatusCode.Should().Be(HttpStatusCode.OK);
        await first.AssertNotice(match.PublicId, "polls", match.Id); await second.AssertNotice(match.PublicId, "polls", match.Id);
        var poll = (await Client.GetFromJsonAsync<PagedResult<PollDto>>($"/api/polls/match/{match.PublicId}"))!.Items.Single();
        (await Client.PostAsJsonAsync($"/api/polls/{poll.PublicId}/vote", new { optionPublicId = poll.Options[0].PublicId })).StatusCode.Should().Be(HttpStatusCode.OK);
        await first.AssertNotice(match.PublicId, "polls", match.Id); await second.AssertNotice(match.PublicId, "polls", match.Id);
        (await Client.GetFromJsonAsync<PagedResult<PollDto>>($"/api/polls/match/{match.PublicId}"))!.Items.Single().TotalVotes.Should().Be(1);
    }

    private sealed class WireClient(WebSocket socket) : IDisposable
    {
        private string _pending = "";
        private int _invocation;
        public Task Send(object message) => socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message) + '\u001e')),
            WebSocketMessageType.Text, true, CancellationToken.None);
        public async Task<JsonElement> Receive()
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            while (true)
            {
                var separator = _pending.IndexOf('\u001e');
                if (separator >= 0)
                {
                    var json = _pending[..separator]; _pending = _pending[(separator + 1)..];
                    using var document = JsonDocument.Parse(json); return document.RootElement.Clone();
                }
                var buffer = new byte[4096];
                var received = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), timeout.Token);
                received.MessageType.Should().Be(WebSocketMessageType.Text);
                _pending += Encoding.UTF8.GetString(buffer, 0, received.Count);
            }
        }
        public async Task<JsonElement> Invoke(string method, params string[] arguments)
        {
            var id = (++_invocation).ToString(); await Send(new { type = 1, invocationId = id, target = method, arguments });
            while (true) { var message = await Receive(); if (message.GetProperty("type").GetInt32() == 3) return message; }
        }
        public async Task AssertNotice(string match, string change, Guid internalId)
        {
            var message = await Receive();
            message.GetProperty("type").GetInt32().Should().Be(1); message.GetProperty("target").GetString().Should().Be("MatchChanged");
            var notice = message.GetProperty("arguments")[0];
            notice.GetProperty("matchPublicId").GetString().Should().Be(match); notice.GetProperty("change").GetString().Should().Be(change);
            notice.GetRawText().Should().NotContain(internalId.ToString());
        }
        public void Dispose() { socket.Abort(); socket.Dispose(); }
    }
}
