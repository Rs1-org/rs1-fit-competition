using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Competition.Tests.IntegrationTests;

public class DeploymentTests(WebApplicationFactory<Program> factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Health_ReportsARevision_ForDeploymentVerification()
    {
        using var response = JsonDocument.Parse(await Client.GetStringAsync("/health"));
        response.RootElement.GetProperty("status").GetString().Should().Be("ok");
        response.RootElement.GetProperty("revision").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Auth_RenewsTokensWithFrontendContract_AndRejectsRevokedRefreshToken()
    {
        var email = $"dev-{Guid.NewGuid():N}@example.invalid";
        var password = $"Dev!aA1{Guid.NewGuid():N}";
        var registration = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email, password, firstName = "Deployment", lastName = "Test", phoneNumber = "061111111",
        });
        registration.StatusCode.Should().Be(HttpStatusCode.OK);
        var login = await Client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        using var auth = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var oldRefresh = auth.RootElement.GetProperty("refreshToken").GetString()!;
        var refreshed = await Client.PostAsJsonAsync("/api/auth/refresh-token", oldRefresh);
        refreshed.StatusCode.Should().Be(HttpStatusCode.OK);
        using var tokens = JsonDocument.Parse(await refreshed.Content.ReadAsStringAsync());
        tokens.RootElement.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        tokens.RootElement.GetProperty("refreshToken").GetString().Should().NotBe(oldRefresh);
        tokens.RootElement.GetProperty("user").GetProperty("email").GetString().Should().Be(email);
        var replay = await Client.PostAsJsonAsync("/api/auth/refresh-token", oldRefresh);
        replay.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/health/ready")]
    public async Task Health_IsAnonymous_AndDoesNotExposeConfiguration(string path)
    {
        var response = await Client.GetAsync(path);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().NotContain("ConnectionString").And.NotContain("Password");
    }

    [Theory]
    [InlineData("http://localhost:4200", true)]
    [InlineData("https://unapproved.example", false)]
    public async Task Cors_PreflightOnlyAllowsConfiguredOrigins(string origin, bool allowed)
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/selection");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "authorization,content-type");
        var response = await Client.SendAsync(request);
        response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins).Should().Be(allowed);
        if (allowed) origins.Should().ContainSingle().Which.Should().Be(origin);
    }

    [Fact]
    public async Task PublicStartup_RemovesPublishedSamplePasswordAndRefreshTokens_ButPreservesRotatedAccounts()
    {
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sample = new ApplicationUser
        {
            Id = Guid.Parse("b1111111-1111-1111-1111-111111111111"),
            UserName = "admin@fit.local",
            PasswordHash = "AQAAAAIAAYagAAAAEFMV98tOqinb6+aZ4euyZT9LY/p40bx8Tu1qC7qWU0N5NhPYn51B28pZbkn/Yj6gFw==",
        };
        var rotated = new ApplicationUser
        {
            Id = Guid.Parse("b2222222-2222-2222-2222-222222222222"),
            UserName = "moderator@fit.local", PasswordHash = "already-rotated", SecurityStamp = "keep",
        };
        db.Users.AddRange(sample, rotated);
        db.RefreshTokens.Add(new RefreshToken { Id = Guid.NewGuid(), UserId = sample.Id, Token = "sample-token" });
        await db.SaveChangesAsync();

        await SampleAccountProtection.DisableDefaultPasswordsAsync(db);
        await SampleAccountProtection.DisableDefaultPasswordsAsync(db); // safe on every cold start

        sample.PasswordHash.Should().BeNull();
        sample.SecurityStamp.Should().NotBeNullOrEmpty();
        rotated.PasswordHash.Should().Be("already-rotated");
        rotated.SecurityStamp.Should().Be("keep");
        db.RefreshTokens.Should().BeEmpty();
    }
}
