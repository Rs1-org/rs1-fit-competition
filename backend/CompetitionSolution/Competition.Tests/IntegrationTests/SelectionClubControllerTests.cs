using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Competition.Tests.IntegrationTests;

public class SelectionClubControllerTests(WebApplicationFactory<Program> factory) : BaseIntegrationTest(factory)
{
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

    [Fact]
    public async Task Writes_EnforceRoles_AndClubValidationReturnsFieldErrors()
    {
        (await Client.PostAsJsonAsync("/api/selection", new { })).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await Client.PutAsJsonAsync("/api/selection/youth", new { })).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        Authenticate("User");
        (await Client.PostAsJsonAsync("/api/selection", new { })).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await Client.PutAsJsonAsync("/api/selection/youth", new { })).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        Authenticate("Moderator");
        var response = await Client.PostAsJsonAsync("/api/selection", new { name = "Youth", abbreviation = "YTH", clubPublicId = new string('a', 32) });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var error = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        error.RootElement.GetProperty("errors").EnumerateArray().Should().Contain(x => x.GetProperty("propertyName").GetString() == "ClubPublicId");
    }

    [Fact]
    public async Task ModeratorCanLinkAndReassignClub_ListAndDetailExposeOnlyPublicClubIdentity()
    {
        var country = new Country { Name = "Country", Slug = "country" };
        var first = new Club { Name = "First", Slug = "first" };
        var second = new Club { Name = "Second", Slug = "second" };
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.AddRange(country, first, second);
            await db.SaveChangesAsync();
        }
        Authenticate("Moderator");
        var response = await Client.PostAsJsonAsync("/api/selection", new
        {
            name = "Youth", abbreviation = "YTH", countryPublicId = country.PublicId, clubPublicId = first.PublicId
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var update = await Client.PutAsJsonAsync("/api/selection/youth", new
        {
            name = "Youth", abbreviation = "YTH", countryPublicId = country.PublicId, clubPublicId = second.PublicId
        });
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        Client.DefaultRequestHeaders.Authorization = null;
        using var detail = JsonDocument.Parse(await Client.GetStringAsync("/api/selection/youth"));
        detail.RootElement.GetProperty("clubName").GetString().Should().Be("Second");
        detail.RootElement.GetProperty("clubSlug").GetString().Should().Be("second");
        detail.RootElement.GetProperty("clubPublicId").GetString().Should().Be(second.PublicId);
        detail.RootElement.TryGetProperty("clubId", out _).Should().BeFalse();
        using var list = JsonDocument.Parse(await Client.GetStringAsync($"/api/selection?clubPublicId={second.PublicId}&pageSize=1"));
        list.RootElement.GetProperty("totalCount").GetInt32().Should().Be(1);
        list.RootElement.GetProperty("items")[0].GetProperty("clubSlug").GetString().Should().Be("second");
        (await Client.GetAsync("/api/selection?clubPublicId=malformed")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
