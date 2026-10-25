using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Competition.Domain.Entities;
using Competition.Domain.Entities.MatchEntities;
using Competition.Infrastructure.Data;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Competition.Tests.IntegrationTests;

public class MatchStatusAndHomeStadiumTests(WebApplicationFactory<Program> factory) : BaseIntegrationTest(factory)
{
    private static readonly string[] ExpectedStatuses = ["Scheduled", "Live", "HalfTime", "Finished", "Postponed", "Cancelled"];

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

    private async Task<(Country Country, Club Club, Stadium Stadium)> SeedGeoAsync()
    {
        var country = new Country { Name = "Bosnia", Slug = "bosnia" };
        var geo = new GeoData();
        var city = new City
        {
            Name = "Sarajevo", Slug = "sarajevo", Country = country, GeoData = geo,
            CountryId = Guid.Empty, GeoDataId = Guid.Empty,
        };
        var stadium = new Stadium
        {
            Name = "Kosevo", Slug = "kosevo", City = city, GeoData = geo,
            DateOfConstruction = new DateTime(1947, 1, 1, 0, 0, 0, DateTimeKind.Utc), Capacity = 34000,
        };
        var club = new Club { Name = "FK Sarajevo", Slug = "fk-sarajevo" };
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.AddRange(country, city, stadium, club);
        await db.SaveChangesAsync();
        return (country, club, stadium);
    }

    [Fact]
    public async Task MatchStatuses_AreSeededOnce_AndReturnedInMatchOrder()
    {
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DataSeeder.SeedMatchStatusesAsync(db);
            await DataSeeder.SeedMatchStatusesAsync(db);
            (await db.MatchStatuses.CountAsync()).Should().Be(ExpectedStatuses.Length);
        }

        var response = await Client.GetAsync("/api/matchstatus");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var page = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        page.RootElement.GetProperty("totalCount").GetInt32().Should().Be(ExpectedStatuses.Length);
        var items = page.RootElement.GetProperty("items").EnumerateArray().ToList();
        items.Select(x => x.GetProperty("name").GetString()).Should().Equal(ExpectedStatuses);
        items.Should().OnlyContain(x => x.GetProperty("publicId").GetString()!.Length > 0);
        items[0].TryGetProperty("id", out _).Should().BeFalse();

        (await Client.GetAsync("/api/matchstatus?pageSize=0")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Match_CanBeCreated_WithSeededStatus()
    {
        var (_, _, stadium) = await SeedGeoAsync();
        string scheduledPublicId;
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DataSeeder.SeedMatchStatusesAsync(db);
            scheduledPublicId = await db.MatchStatuses.Where(x => x.Slug == "scheduled").Select(x => x.PublicId).SingleAsync();
        }

        Authenticate("Moderator");
        var response = await Client.PostAsJsonAsync("/api/match/create", new
        {
            slug = "derby", date = DateTime.UtcNow.AddDays(7), statusPublicId = scheduledPublicId, stadiumPublicId = stadium.PublicId,
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Selection_HomeStadium_IsOptional_ExposedAsPublicLink_AndValidated()
    {
        var (country, club, stadium) = await SeedGeoAsync();
        Authenticate("Moderator");

        var unknown = await Client.PostAsJsonAsync("/api/selection", new
        {
            name = "Seniors", abbreviation = "SEN", countryPublicId = country.PublicId, clubPublicId = club.PublicId,
            homeStadiumPublicId = new string('a', 32),
        });
        unknown.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using (var error = JsonDocument.Parse(await unknown.Content.ReadAsStringAsync()))
            error.RootElement.GetProperty("errors").EnumerateArray()
                .Should().Contain(x => x.GetProperty("propertyName").GetString() == "HomeStadiumPublicId");

        var created = await Client.PostAsJsonAsync("/api/selection", new
        {
            name = "Seniors", abbreviation = "SEN", countryPublicId = country.PublicId, clubPublicId = club.PublicId,
            homeStadiumPublicId = stadium.PublicId,
        });
        created.StatusCode.Should().Be(HttpStatusCode.OK);

        Client.DefaultRequestHeaders.Authorization = null;
        using (var detail = JsonDocument.Parse(await Client.GetStringAsync("/api/selection/seniors")))
        {
            detail.RootElement.GetProperty("stadiumName").GetString().Should().Be("Kosevo");
            detail.RootElement.GetProperty("stadiumSlug").GetString().Should().Be("kosevo");
            detail.RootElement.GetProperty("homeStadiumPublicId").GetString().Should().Be(stadium.PublicId);
            detail.RootElement.TryGetProperty("homeStadiumId", out _).Should().BeFalse();
        }

        // Saving without a stadium clears the link.
        Authenticate("Moderator");
        var cleared = await Client.PutAsJsonAsync("/api/selection/seniors", new
        {
            name = "Seniors", abbreviation = "SEN", countryPublicId = country.PublicId, clubPublicId = club.PublicId,
        });
        cleared.StatusCode.Should().Be(HttpStatusCode.OK);
        using var updated = JsonDocument.Parse(await cleared.Content.ReadAsStringAsync());
        updated.RootElement.GetProperty("stadiumSlug").ValueKind.Should().Be(JsonValueKind.Null);
    }
}
