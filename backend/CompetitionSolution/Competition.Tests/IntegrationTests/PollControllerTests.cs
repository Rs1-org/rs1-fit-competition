using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Competition.Application.Auth.Interfaces;
using Competition.Application.Common.Models;
using Competition.Application.Polls;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Competition.Tests.IntegrationTests;

public class PollControllerTests(WebApplicationFactory<Program> factory) : BaseIntegrationTest(factory)
{
    private async Task<Match> Match()
    {
        using var scope = ServiceProvider.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var match = StatisticTestData.NewMatch(); db.Matches.Add(match); await db.SaveChangesAsync(); return match;
    }
    private async Task<ApplicationUser> Login(string role)
    {
        using var scope = ServiceProvider.CreateScope(); var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var email = $"poll-{Guid.NewGuid():N}@example.invalid";
        var user = new ApplicationUser { Email = email, UserName = email, FirstName = "Poll", LastName = "Test" };
        (await manager.CreateAsync(user)).Succeeded.Should().BeTrue(); await manager.AddToRoleAsync(user, role);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>().GenerateJwtToken(user, [role])); return user;
    }
    private static object Input(string[]? options = null, string question = "Who will win?") => new
    {
        question, options = options ?? ["Home", "Away"], opensAt = DateTimeOffset.UtcNow.AddMinutes(-1), closesAt = DateTimeOffset.UtcNow.AddHours(1),
    };
    private async Task<PollDto> Create(Match match)
    {
        var response = await Client.PostAsJsonAsync($"/api/polls/match/{match.PublicId}", Input());
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        var saved = (await response.Content.ReadFromJsonAsync<SavedPollDto>())!;
        return (await Client.GetFromJsonAsync<PagedResult<PollDto>>($"/api/polls/match/{match.PublicId}"))!.Items.Single(x => x.PublicId == saved.PublicId);
    }
    [Fact]
    public async Task LoggedInUsersVoteOnce_ResultsAndPercentagesArePublic_ViewerIdentityCannotBeSpoofed()
    {
        var match = await Match(); await Login("Moderator"); var poll = await Create(match);
        poll.IsOpen.Should().BeTrue(); poll.TotalVotes.Should().Be(0); poll.Options.Should().OnlyContain(x => x.Percentage == 0);
        var first = await Login("User"); var option = poll.Options[0].PublicId;
        (await Client.PostAsJsonAsync($"/api/polls/{poll.PublicId}/vote", new { optionPublicId = option, userId = Guid.NewGuid() })).StatusCode.Should().Be(HttpStatusCode.OK);
        (await Client.PostAsJsonAsync($"/api/polls/{poll.PublicId}/vote", new { optionPublicId = poll.Options[1].PublicId })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var mine = (await Client.GetFromJsonAsync<PagedResult<PollDto>>($"/api/polls/match/{match.PublicId}"))!.Items.Single();
        mine.MyOptionPublicId.Should().Be(option); mine.Options[0].Votes.Should().Be(1); mine.Options[0].Percentage.Should().Be(100);
        await Login("User");
        (await Client.PostAsJsonAsync($"/api/polls/{poll.PublicId}/vote", new { optionPublicId = poll.Options[1].PublicId })).StatusCode.Should().Be(HttpStatusCode.OK);
        Client.DefaultRequestHeaders.Authorization = null;
        var publicPage = (await Client.GetFromJsonAsync<PagedResult<PollDto>>($"/api/polls/match/{match.PublicId}?viewerId={first.Id}"))!;
        var result = publicPage.Items.Single(); result.TotalVotes.Should().Be(2); result.MyOptionPublicId.Should().BeNull();
        result.Options.Should().OnlyContain(x => x.Votes == 1 && x.Percentage == 50);
        var json = await Client.GetStringAsync($"/api/polls/match/{match.PublicId}");
        json.Should().NotContain(first.Id.ToString()).And.NotContain(first.Email!).And.NotContain(match.Id.ToString());
    }
    [Theory]
    [InlineData("User")] [InlineData("Manager")]
    public async Task PollCreation_RequiresModeratorOrAdmin_AndVotesRequireAuthentication(string role)
    {
        var match = await Match();
        (await Client.PostAsJsonAsync($"/api/polls/match/{match.PublicId}", Input())).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await Client.PostAsJsonAsync($"/api/polls/{new string('a', 32)}/vote", new { optionPublicId = new string('b', 32) })).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        await Login(role);
        (await Client.PostAsJsonAsync($"/api/polls/match/{match.PublicId}", Input())).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    [Fact]
    public async Task InvalidQuestionsOptionsAndTimeWindows_AreRejected()
    {
        var match = await Match(); await Login("Admin");
        foreach (var input in new[] { Input(["Only"]), Input(["Home", " home "]), Input([" ", "Away"]), Input([new string('a', 101), "Away"]), Input(question: " "), Input(question: new string('q', 201)) })
            (await Client.PostAsJsonAsync($"/api/polls/match/{match.PublicId}", input)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        foreach (var closes in new[] { DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow.AddMinutes(30) })
            (await Client.PostAsJsonAsync($"/api/polls/match/{match.PublicId}", new { question = "Question?", options = new[] { "A", "B" }, opensAt = DateTimeOffset.UtcNow.AddHours(1), closesAt = closes })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.PostAsJsonAsync($"/api/polls/match/{match.PublicId}", new { question = "Question?", options = (string[]?)null, opensAt = DateTimeOffset.UtcNow, closesAt = DateTimeOffset.UtcNow.AddHours(1) })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task ScheduledAndClosedPollsRejectVotes_AndOptionsMustBelongToThePoll()
    {
        var match = await Match(); await Login("Moderator"); var open = await Create(match); var other = await Create(match);
        (await Client.PostAsJsonAsync($"/api/polls/{open.PublicId}/vote", new { optionPublicId = other.Options[0].PublicId })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var scope = ServiceProvider.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = SystemClock.Instance.GetCurrentInstant();
        foreach (var offset in new[] { -2, 1 })
        {
            var poll = new Poll { MatchId = match.Id, Question = "Outside voting window?", OpensAt = now + Duration.FromHours(offset), ClosesAt = now + Duration.FromHours(offset + 1),
                Options = [new PollOption { Label = "A", SortOrder = 0 }, new PollOption { Label = "B", SortOrder = 1 }] };
            db.Polls.Add(poll); await db.SaveChangesAsync();
            (await Client.PostAsJsonAsync($"/api/polls/{poll.PublicId}/vote", new { optionPublicId = poll.Options.First().PublicId })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
    [Fact]
    public async Task PollPagingAndSoftDeletedMatch_AreRespected()
    {
        var match = await Match(); await Login("Moderator"); await Create(match); await Create(match);
        var page = (await Client.GetFromJsonAsync<PagedResult<PollDto>>($"/api/polls/match/{match.PublicId}?page=2&pageSize=1"))!;
        page.TotalCount.Should().Be(2); page.Items.Should().ContainSingle(); page.Page.Should().Be(2);
        (await Client.GetAsync($"/api/polls/match/{match.PublicId}?pageSize=51")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var scope = ServiceProvider.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = await db.Matches.FindAsync(match.Id); entity!.DeletedAt = SystemClock.Instance.GetCurrentInstant(); await db.SaveChangesAsync();
        (await Client.GetAsync($"/api/polls/match/{match.PublicId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Client.PostAsJsonAsync($"/api/polls/{page.Items.Single().PublicId}/vote", new { optionPublicId = page.Items.Single().Options[0].PublicId })).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
