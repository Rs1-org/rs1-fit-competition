using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Competition.Domain.Entities;
using Competition.Domain.Entities.CompetitionEntities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using Xunit;
using CompetitionEntity = Competition.Domain.Entities.CompetitionEntities.Competition;

namespace Competition.Tests.IntegrationTests;

/// <summary>COMP-65: what the match form relies on — generated slugs, role checks, detail links.</summary>
public class MatchWriteTests(WebApplicationFactory<Program> factory) : BaseIntegrationTest(factory)
{
    private record Seed(MatchStatus Status, Stadium Stadium, Selection Home, Selection Away, Round Round);

    private void Authenticate(string role)
    {
        var options = ServiceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);
        var parameters = options.TokenValidationParameters;
        var token = new JwtSecurityToken(parameters.ValidIssuer, parameters.ValidAudience,
            [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()), new Claim(ClaimTypes.Role, role)],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(parameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256));
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
    }

    private async Task<Seed> SeedAsync()
    {
        var country = new Country { Name = "Bosnia", Slug = "bosnia" };
        var geo = new GeoData();
        var city = new City { Name = "Sarajevo", Slug = "sarajevo", Country = country, GeoData = geo, CountryId = Guid.Empty, GeoDataId = Guid.Empty };
        var stadium = new Stadium { Name = "Grbavica", Slug = "grbavica", City = city, GeoData = geo, DateOfConstruction = DateTime.UtcNow, Capacity = 13000 };
        var status = new MatchStatus { Name = "Scheduled", Slug = "test-scheduled" };
        var home = new Selection { Name = "Zeljeznicar", Abbreviation = "ZEL", Country = country, Slug = "zeljeznicar" };
        var away = new Selection { Name = "Sarajevo", Abbreviation = "SAR", Country = country, Slug = "fk-sarajevo" };
        var competition = new CompetitionEntity { Name = "Premier League", Slug = "premier-league" };
        var season = new Season
        {
            Name = "2026/27", Slug = "premier-league-2026-27", Competition = competition,
            StartDate = new LocalDate(2026, 7, 1), EndDate = new LocalDate(2027, 6, 1),
        };
        var round = new Round { Name = "Round 1", Slug = "premier-league-2026-27-round-1", Number = 1, Season = season };

        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.AddRange(country, city, stadium, status, home, away, round);
        await db.SaveChangesAsync();
        return new Seed(status, stadium, home, away, round);
    }

    private static object Body(Seed s, DateTime date, string? slug = null) => new
    {
        slug, date, statusPublicId = s.Status.PublicId, stadiumPublicId = s.Stadium.PublicId,
        homeSelectionPublicId = s.Home.PublicId, awaySelectionPublicId = s.Away.PublicId, roundPublicId = s.Round.PublicId,
    };

    [Fact]
    public async Task Create_WithoutSlug_GeneratesUniqueSlug_AndDetailsCarryLinks()
    {
        var seed = await SeedAsync();
        var date = new DateTime(2030, 3, 14, 18, 0, 0, DateTimeKind.Utc);
        Authenticate("Moderator");

        var first = await Client.PostAsJsonAsync("/api/match/create", Body(seed, date));
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        var second = await Client.PostAsJsonAsync("/api/match/create", Body(seed, date));
        second.StatusCode.Should().Be(HttpStatusCode.OK);

        using var created = JsonDocument.Parse(await first.Content.ReadAsStringAsync());
        using var duplicate = JsonDocument.Parse(await second.Content.ReadAsStringAsync());
        created.RootElement.GetProperty("slug").GetString().Should().Be("zeljeznicar-vs-sarajevo-2030-03-14");
        duplicate.RootElement.GetProperty("slug").GetString().Should().Be("zeljeznicar-vs-sarajevo-2030-03-14-2");

        Client.DefaultRequestHeaders.Authorization = null;
        using var detail = JsonDocument.Parse(await Client.GetStringAsync("/api/match/get/zeljeznicar-vs-sarajevo-2030-03-14"));
        var root = detail.RootElement;
        root.GetProperty("stadiumSlug").GetString().Should().Be("grbavica");
        root.GetProperty("homeSelectionSlug").GetString().Should().Be("zeljeznicar");
        root.GetProperty("roundSlug").GetString().Should().Be("premier-league-2026-27-round-1");
        root.GetProperty("seasonPublicId").GetString().Should().Be(seed.Round.Season!.PublicId);
        root.GetProperty("competitionPublicId").GetString().Should().Be(seed.Round.Season!.Competition!.PublicId);
    }

    [Fact]
    public async Task Create_WithTakenOrMalformedSlug_ReturnsFieldError()
    {
        var seed = await SeedAsync();
        var date = DateTime.UtcNow.AddDays(10);
        Authenticate("Admin");
        (await Client.PostAsJsonAsync("/api/match/create", Body(seed, date, "derby"))).StatusCode.Should().Be(HttpStatusCode.OK);

        foreach (var slug in new[] { "derby", "Not A Slug" })
        {
            var response = await Client.PostAsJsonAsync("/api/match/create", Body(seed, date, slug));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            using var error = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            error.RootElement.GetProperty("errors").EnumerateArray()
                .Should().Contain(x => x.GetProperty("propertyName").GetString() == "Slug");
        }
    }

    [Fact]
    public async Task Writes_RequireAdminOrModerator()
    {
        var seed = await SeedAsync();
        Authenticate("User");
        (await Client.PostAsJsonAsync("/api/match/create", Body(seed, DateTime.UtcNow.AddDays(3))))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await Client.PutAsJsonAsync("/api/match/update/any", Body(seed, DateTime.UtcNow.AddDays(3))))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SelectionList_ExposesPublicId_ForMatchForm()
    {
        var seed = await SeedAsync();
        using var list = JsonDocument.Parse(await Client.GetStringAsync("/api/selection?name=zeljeznicar"));
        list.RootElement.GetProperty("items")[0].GetProperty("publicId").GetString().Should().Be(seed.Home.PublicId);
    }
}
