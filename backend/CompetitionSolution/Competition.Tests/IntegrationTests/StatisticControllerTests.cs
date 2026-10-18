using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Competition.Application.Auth.Interfaces;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.StatisticDto;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Competition.Tests.IntegrationTests;

public class StatisticControllerTests(WebApplicationFactory<Program> factory) : BaseIntegrationTest(factory)
{
    private static TeamStatisticValues Values(int possession, int shots = 12) => new(shots, 5, 0, 2, 4, 3, possession);

    private void Authenticate(string role)
    {
        using var scope = ServiceProvider.CreateScope();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            jwt.GenerateJwtToken(new ApplicationUser { Id = Guid.NewGuid(), Email = "test@example.invalid" }, [role]));
    }

    private async Task<Match> SeedMatch(Instant? date = null)
    {
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var match = StatisticTestData.NewMatch();
        if (date.HasValue) match.Date = date.Value;
        db.Matches.Add(match);
        await db.SaveChangesAsync();
        return match;
    }

    [Fact]
    public async Task AuthorizedCrud_SavesBothTeams_ReturnsOnlyPublicIds_AndSoftDeletes()
    {
        var match = await SeedMatch();
        Authenticate("Moderator");
        var created = await Client.PostAsJsonAsync("/api/statistic", new
        {
            matchPublicId = match.PublicId, home = Values(60), away = Values(40, 8),
        });
        created.StatusCode.Should().Be(HttpStatusCode.OK);
        var statistic = (await created.Content.ReadFromJsonAsync<StatisticDto>())!;
        statistic.Home!.SelectionPublicId.Should().Be(match.HomeSelection!.PublicId);
        statistic.Away!.SelectionPublicId.Should().Be(match.AwaySelection!.PublicId);
        statistic.Home.TotalShots.Should().Be(12);
        var json = await created.Content.ReadAsStringAsync();
        json.Should().NotContain(match.Id.ToString()).And.NotContain(match.HomeSelectionId!.Value.ToString());
        json.Should().NotContain("\"matchId\"").And.NotContain("\"selectionId\"");

        (await Client.PostAsJsonAsync("/api/statistic", new
        {
            matchPublicId = match.PublicId, home = Values(60), away = Values(40),
        })).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var update = await Client.PutAsJsonAsync($"/api/statistic/{statistic.PublicId}", new { home = Values(55, 20), away = Values(45) });
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        (await update.Content.ReadFromJsonAsync<StatisticDto>())!.Home!.TotalShots.Should().Be(20);
        (await Client.DeleteAsync($"/api/statistic/{statistic.PublicId}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        Client.DefaultRequestHeaders.Authorization = null;
        var detail = await Client.GetFromJsonAsync<StatisticDto>($"/api/statistic/match/{match.PublicId}");
        detail!.Home!.Possession.Should().Be(55);
        var page = await Client.GetFromJsonAsync<PagedResult<StatisticDto>>($"/api/statistic?matchPublicId={match.PublicId}&pageSize=1");
        page!.TotalCount.Should().Be(1);
        page.Items.Should().ContainSingle();

        Authenticate("Admin");
        (await Client.DeleteAsync($"/api/statistic/{statistic.PublicId}")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await Client.GetAsync($"/api/statistic/{statistic.PublicId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Statistics.IgnoreQueryFilters().CountAsync()).Should().Be(1);
        (await db.TeamStatistics.IgnoreQueryFilters().CountAsync()).Should().Be(2);
        (await db.TeamStatistics.CountAsync()).Should().Be(0);
    }

    [Theory]
    [InlineData(-1, 40, 12, 5)]
    [InlineData(60, 50, 12, 5)]
    [InlineData(60, 40, 4, 5)]
    public async Task InvalidStatistics_Return400_WithoutPartialRows(int homePossession, int awayPossession, int shots, int target)
    {
        var match = await SeedMatch();
        Authenticate("Admin");
        var response = await Client.PostAsJsonAsync("/api/statistic", new
        {
            matchPublicId = match.PublicId, home = new TeamStatisticValues(shots, target, 0, 0, 0, 0, homePossession),
            away = Values(awayPossession),
        });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Statistics.CountAsync()).Should().Be(0);
        (await db.TeamStatistics.CountAsync()).Should().Be(0);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    [InlineData("Manager", HttpStatusCode.Forbidden)]
    public async Task UnprivilegedWrites_AreRejected(string? role, HttpStatusCode expected)
    {
        if (role is not null) Authenticate(role);
        (await Client.PostAsJsonAsync("/api/statistic", new { })).StatusCode.Should().Be(expected);
        (await Client.PutAsJsonAsync("/api/statistic/" + new string('a', 32), new { })).StatusCode.Should().Be(expected);
        (await Client.DeleteAsync("/api/statistic/" + new string('a', 32))).StatusCode.Should().Be(expected);
    }

    [Fact]
    public async Task IncompleteMatchAndInvalidPaging_AreRejected()
    {
        var match = await SeedMatch();
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var row = await db.Matches.SingleAsync();
            row.AwaySelectionId = null;
            await db.SaveChangesAsync();
        }
        Authenticate("Admin");
        (await Client.PostAsJsonAsync("/api/statistic", new
        {
            matchPublicId = match.PublicId, home = Values(60), away = Values(40),
        })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.GetAsync("/api/statistic?page=0")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.GetAsync("/api/statistic?minTotalShots=-1")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<StatisticDto> CreateStatistic(Match match, int shots = 12)
    {
        var response = await Client.PostAsJsonAsync("/api/statistic", new
        {
            matchPublicId = match.PublicId, home = Values(60, shots), away = Values(40, 8),
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<StatisticDto>())!;
    }

    [Fact]
    public async Task AllFiveFilters_RunBeforePaging_AndPreserveTotals()
    {
        var first = await SeedMatch(Instant.FromUtc(2025, 1, 1, 12, 0));
        var second = await SeedMatch(Instant.FromUtc(2026, 1, 1, 12, 0));
        Authenticate("Admin");
        await CreateStatistic(first);
        await CreateStatistic(second, 24);
        var page = await Client.GetFromJsonAsync<PagedResult<StatisticDto>>("/api/statistic?pageSize=1");
        page!.TotalCount.Should().Be(2);
        page.Items.Should().ContainSingle().Which.MatchPublicId.Should().Be(second.PublicId);
        var filters = new Dictionary<string, string>
        {
            [$"matchPublicId={first.PublicId}"] = first.PublicId,
            [$"selectionPublicId={first.HomeSelection!.PublicId}"] = first.PublicId,
            ["dateFrom=2026-01-01T00:00:00Z"] = second.PublicId,
            ["dateTo=2025-01-02T00:00:00Z"] = first.PublicId,
            ["minTotalShots=20"] = second.PublicId,
        };
        foreach (var (filter, expectedMatch) in filters)
        {
            var result = (await Client.GetFromJsonAsync<PagedResult<StatisticDto>>($"/api/statistic?{filter}&pageSize=1"))!;
            result.TotalCount.Should().Be(1);
            result.Items.Should().ContainSingle().Which.MatchPublicId.Should().Be(expectedMatch);
        }
    }

    [Fact]
    public async Task UpdateAfterMatchSelectionChange_HidesOldTeam_AndPreservesMatchTimestamp()
    {
        var match = await SeedMatch();
        Authenticate("Admin");
        var statistic = await CreateStatistic(match);
        string newTeamPublicId;
        Instant modifiedAt;
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var row = await db.Matches.SingleAsync();
            var newTeam = new Selection { Name = "New home", Abbreviation = "NEW", CountryId = match.HomeSelection!.CountryId };
            db.Selections.Add(newTeam);
            row.HomeSelection = newTeam;
            await db.SaveChangesAsync();
            newTeamPublicId = newTeam.PublicId;
            modifiedAt = row.ModifiedAt;
        }
        var update = await Client.PutAsJsonAsync($"/api/statistic/{statistic.PublicId}", new { home = Values(55, 20), away = Values(45) });
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = (await update.Content.ReadFromJsonAsync<StatisticDto>())!;
        updated.Home!.SelectionPublicId.Should().Be(newTeamPublicId);
        updated.Home.TotalShots.Should().Be(20);
        using var verification = ServiceProvider.CreateScope();
        var context = verification.ServiceProvider.GetRequiredService<AppDbContext>();
        (await context.TeamStatistics.CountAsync()).Should().Be(2);
        (await context.TeamStatistics.IgnoreQueryFilters().CountAsync()).Should().Be(3);
        (await context.Matches.SingleAsync()).ModifiedAt.Should().Be(modifiedAt);
    }

    [Fact]
    public async Task DeletedMatch_HidesStatisticsFromPublicDetailsAndList()
    {
        var match = await SeedMatch();
        Authenticate("Admin");
        var statistic = await CreateStatistic(match);
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Remove(await db.Matches.SingleAsync());
            await db.SaveChangesAsync();
        }
        (await Client.GetAsync($"/api/statistic/{statistic.PublicId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Client.GetAsync($"/api/statistic/match/{match.PublicId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Client.GetFromJsonAsync<PagedResult<StatisticDto>>("/api/statistic"))!.TotalCount.Should().Be(0);
    }
}
