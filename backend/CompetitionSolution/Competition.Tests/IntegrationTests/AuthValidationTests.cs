using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Competition.Tests.IntegrationTests;

public class AuthValidationTests(WebApplicationFactory<Program> factory) : BaseIntegrationTest(factory)
{
    private static object Registration(string email, string password, string? confirmation) => new
    {
        email, password, confirmPassword = confirmation, firstName = "Auth", lastName = "Test", phoneNumber = "061111111",
    };

    [Theory]
    [InlineData("invalid", "Valid!aA123", "Valid!aA123", "email")]
    [InlineData("auth@example.invalid", "short", "short", "password")]
    [InlineData("auth@example.invalid", "Valid!aA123", "different", "confirmPassword")]
    [InlineData("auth@example.invalid", "Valid!aA123", null, "confirmPassword")]
    public async Task Register_RejectsInvalidFields_WithReadableValidationErrors(
        string email, string password, string? confirmation, string field)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", Registration(email, password, confirmation));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("errors").EnumerateArray()
            .Should().Contain(x => x.GetProperty("propertyName").GetString() == field);
        (await response.Content.ReadAsStringAsync()).Should().NotContain(password);
    }

    [Fact]
    public async Task Register_RejectsDuplicateEmailCaseInsensitively()
    {
        var email = $"auth-{Guid.NewGuid():N}@example.invalid";
        var password = $"Test!aA1{Guid.NewGuid():N}";
        (await Client.PostAsJsonAsync("/api/auth/register", Registration(email, password, password)))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        var duplicate = await Client.PostAsJsonAsync("/api/auth/register", Registration(email.ToUpperInvariant(), password, password));
        duplicate.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await duplicate.Content.ReadAsStringAsync()).Should().Contain("already registered");
    }

    [Theory]
    [InlineData("/api/auth/login")]
    [InlineData("/api/auth/logout")]
    public async Task AuthCommands_RejectEmptyRequiredFields(string path)
    {
        (await Client.PostAsJsonAsync(path, new { email = "", password = "", token = "" }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logout_RevokesInsteadOfDeleting_AndRejectsReplay()
    {
        var (_, refresh) = await RegisterAndLogin();
        (await Client.PostAsJsonAsync("/api/auth/logout", new { token = refresh }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var token = await db.RefreshTokens.SingleAsync(x => x.Token == refresh);
        token.IsRevoked.Should().BeTrue();
        token.RevokedAt.Should().NotBeNull();
        var revokedAt = token.RevokedAt;
        (await Client.PostAsJsonAsync("/api/auth/logout", new { token = refresh })).StatusCode.Should().Be(HttpStatusCode.OK);
        await db.Entry(token).ReloadAsync();
        token.RevokedAt.Should().Be(revokedAt);
        (await Client.PostAsJsonAsync("/api/auth/refresh-token", refresh)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Refresh_RejectsExpiredOrUnknownToken(bool expired)
    {
        var (_, refresh) = await RegisterAndLogin();
        if (expired)
        {
            using var scope = ServiceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var token = await db.RefreshTokens.SingleAsync(x => x.Token == refresh);
            token.Expires = DateTime.UtcNow.AddDays(-1);
            await db.SaveChangesAsync();
        }
        (await Client.PostAsJsonAsync("/api/auth/refresh-token", expired ? refresh : "unknown"))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task InactiveAccount_CannotLoginOrRefresh()
    {
        var (email, refresh) = await RegisterAndLogin();
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync(x => x.Email == email);
        user.IsActive = false;
        await db.SaveChangesAsync();

        (await Client.PostAsJsonAsync("/api/auth/login", new { email, password = "Test!aA123" }))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await Client.PostAsJsonAsync("/api/auth/refresh-token", refresh)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<(string Email, string Refresh)> RegisterAndLogin()
    {
        var email = $"auth-{Guid.NewGuid():N}@example.invalid";
        var password = "Test!aA123";
        (await Client.PostAsJsonAsync("/api/auth/register", Registration(email, password, password)))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        using var response = await Client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return (email, json.RootElement.GetProperty("refreshToken").GetString()!);
    }
}
