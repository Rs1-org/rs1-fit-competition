using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Competition.Application.Auth.Interfaces;
using Competition.Application.Common.Models;
using Competition.Application.Events;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Domain.Entities.Person;
using Competition.Infrastructure.Data;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Competition.Tests.IntegrationTests;

public class EventControllerTests(WebApplicationFactory<Program> factory) : BaseIntegrationTest(factory)
{
    private record Fixture(Match Match, EventLookupsDto Lookups, Player First, Player Second);
    private async Task<Fixture> Setup()
    {
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var match = StatisticTestData.NewMatch();
        Player Player(string name) => new() { FirstName = name, LastName = "Event test", Picture = [], DateOfBirth = new LocalDate(2000, 1, 1) };
        var first = Player("First"); var second = Player("Second");
        db.Matches.Add(match);
        db.PlayerSelectionHistories.AddRange(new PlayerSelectionHistory { Player = first, Selection = match.HomeSelection },
            new PlayerSelectionHistory { Player = second, Selection = match.HomeSelection });
        await db.SaveChangesAsync();
        return new(match, (await Client.GetFromJsonAsync<EventLookupsDto>("/api/events/lookups"))!, first, second);
    }
    private async Task Authenticate(string role)
    {
        using var scope = ServiceProvider.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var email = $"events-{Guid.NewGuid():N}@example.invalid";
        var user = new ApplicationUser { Email = email, UserName = email, FirstName = "Events", LastName = "Test" };
        (await manager.CreateAsync(user)).Succeeded.Should().BeTrue();
        await manager.AddToRoleAsync(user, role);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>().GenerateJwtToken(user, [role]));
    }
    private static CreateEventCommand Input(Fixture data, string code = "goal", int minute = 30) => new()
    {
        MatchPublicId = data.Match.PublicId, EventTypePublicId = data.Lookups.EventTypes.Single(x => x.Code == code).PublicId,
        MatchPartPublicId = data.Lookups.MatchParts.Single(x => x.Code == "first-half").PublicId,
        SelectionPublicId = data.Match.HomeSelection!.PublicId, PlayerPublicId = data.First.PublicId, Minute = minute,
    };
    private async Task<SavedEventDto> Create(CreateEventCommand input)
    {
        var result = await Client.PostAsJsonAsync("/api/events", input);
        result.StatusCode.Should().Be(HttpStatusCode.OK, await result.Content.ReadAsStringAsync());
        return (await result.Content.ReadFromJsonAsync<SavedEventDto>())!;
    }

    [Fact]
    public async Task ModeratorCrud_UsesPublicIds_AllFiltersAndPaging_AndSoftDeletes()
    {
        var data = await Setup(); await Authenticate("Moderator");
        var input = Input(data); input.AddedMinute = 2; input.Note = "  Recorded event  ";
        var first = await Create(input); await Create(Input(data, minute: 31));
        var query = $"/api/events?matchPublicId={data.Match.PublicId}&eventTypePublicId={input.EventTypePublicId}"
            + $"&selectionPublicId={input.SelectionPublicId}&playerPublicId={input.PlayerPublicId}&minuteFrom=30&minuteTo=31&pageSize=1&page=2";
        var page = (await Client.GetFromJsonAsync<PagedResult<EventDto>>(query))!;
        page.TotalCount.Should().Be(2); page.Items.Single().Minute.Should().Be(31); page.Page.Should().Be(2);
        var before = (await Client.GetFromJsonAsync<EventDto>($"/api/events/{first.PublicId}"))!;
        before.Note.Should().Be("Recorded event"); before.AddedMinute.Should().Be(2);
        var json = await Client.GetStringAsync($"/api/events/{first.PublicId}");
        json.Should().NotContain(data.Match.Id.ToString()).And.NotContain(data.First.Id.ToString());
        input.Minute = 40;
        (await Client.PutAsJsonAsync($"/api/events/{first.PublicId}", input)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await Client.GetFromJsonAsync<EventDto>($"/api/events/{first.PublicId}"))!.Minute.Should().Be(40);
        (await Client.DeleteAsync($"/api/events/{first.PublicId}")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await Client.GetAsync($"/api/events/{first.PublicId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var scope = ServiceProvider.CreateScope();
        (await scope.ServiceProvider.GetRequiredService<AppDbContext>().Events.IgnoreQueryFilters()
            .SingleAsync(x => x.PublicId == first.PublicId)).DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Substitution_RequiresDifferentTeamPlayers_AndPlayerFilterIncludesPlayerOn()
    {
        var data = await Setup(); await Authenticate("Admin");
        var input = Input(data, "substitution");
        (await Client.PostAsJsonAsync("/api/events", input)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        input.RelatedPlayerPublicId = data.First.PublicId;
        (await Client.PostAsJsonAsync("/api/events", input)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        input.RelatedPlayerPublicId = data.Second.PublicId;
        var saved = await Create(input);
        var page = (await Client.GetFromJsonAsync<PagedResult<EventDto>>($"/api/events?playerPublicId={data.Second.PublicId}"))!;
        page.Items.Single().PublicId.Should().Be(saved.PublicId);
        input.EventTypePublicId = data.Lookups.EventTypes.Single(x => x.Code == "goal").PublicId;
        (await Client.PostAsJsonAsync("/api/events", input)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InvalidTeamMembership_MovingMatch_AndDeletedReferences_AreRejected()
    {
        var data = await Setup(); var other = await Setup(); await Authenticate("Moderator");
        var input = Input(data); var saved = await Create(input);
        input.SelectionPublicId = data.Match.AwaySelection!.PublicId;
        (await Client.PostAsJsonAsync("/api/events", input)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        input.SelectionPublicId = other.Match.HomeSelection!.PublicId;
        (await Client.PostAsJsonAsync("/api/events", input)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        input = Input(other);
        (await Client.PutAsJsonAsync($"/api/events/{saved.PublicId}", input)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var scope = ServiceProvider.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var player = await db.Players.SingleAsync(x => x.PublicId == data.First.PublicId);
        player.DeletedAt = SystemClock.Instance.GetCurrentInstant(); await db.SaveChangesAsync();
        (await Client.PostAsJsonAsync("/api/events", Input(data))).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(-1, 0)] [InlineData(131, 0)] [InlineData(45, -1)] [InlineData(45, 31)]
    public async Task InvalidMinutes_AreRejected(int minute, int added)
    {
        var data = await Setup(); await Authenticate("Moderator");
        var input = Input(data, minute: minute); input.AddedMinute = added;
        (await Client.PostAsJsonAsync("/api/events", input)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MalformedReferences_Paging_MinuteRange_AndLongNotes_AreRejected()
    {
        var data = await Setup(); await Authenticate("Moderator"); var input = Input(data);
        input.PlayerPublicId = "";
        (await Client.PostAsJsonAsync("/api/events", input)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        input.PlayerPublicId = null; input.Note = new string('x', 501);
        (await Client.PostAsJsonAsync("/api/events", input)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        foreach (var filter in new[] { "page=0", "pageSize=101", "minuteFrom=40&minuteTo=30", "matchPublicId=bad", "minuteFrom=-1" })
            (await Client.GetAsync($"/api/events?{filter}")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("User")] [InlineData("Manager")]
    public async Task OrdinaryRoles_CanReadButCannotWrite(string role)
    {
        var data = await Setup(); await Authenticate("Moderator"); var saved = await Create(Input(data));
        Client.DefaultRequestHeaders.Authorization = null;
        (await Client.GetAsync("/api/events")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await Client.PostAsJsonAsync("/api/events", Input(data))).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        await Authenticate(role);
        (await Client.PostAsJsonAsync("/api/events", Input(data))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await Client.PutAsJsonAsync($"/api/events/{saved.PublicId}", Input(data))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await Client.DeleteAsync($"/api/events/{saved.PublicId}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Lookups_AreSeededWithoutDemoAccounts_AndSeedingIsIdempotent()
    {
        var before = (await Client.GetFromJsonAsync<EventLookupsDto>("/api/events/lookups"))!;
        before.EventTypes.Should().HaveCount(7); before.MatchParts.Should().HaveCount(4);
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await DataSeeder.SeedEventLookupsAsync(db); await DataSeeder.SeedEventLookupsAsync(db);
        var after = (await Client.GetFromJsonAsync<EventLookupsDto>("/api/events/lookups"))!;
        after.Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task ContextReturnsOnlyActiveMembers_AndDeletedMatchHidesEvents()
    {
        var data = await Setup(); await Authenticate("Moderator"); var saved = await Create(Input(data));
        var context = (await Client.GetFromJsonAsync<EventContextDto>($"/api/events/match/{data.Match.PublicId}/context"))!;
        context.Selections.Should().HaveCount(2);
        context.Selections.Single(x => x.PublicId == data.Match.HomeSelection!.PublicId).Players.Should().HaveCount(2);
        using var scope = ServiceProvider.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var match = await db.Matches.SingleAsync(x => x.Id == data.Match.Id);
        match.DeletedAt = SystemClock.Instance.GetCurrentInstant(); await db.SaveChangesAsync();
        (await Client.GetAsync($"/api/events/{saved.PublicId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Client.GetFromJsonAsync<PagedResult<EventDto>>($"/api/events?matchPublicId={data.Match.PublicId}"))!.TotalCount.Should().Be(0);
        (await Client.GetAsync($"/api/events/match/{data.Match.PublicId}/context")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
