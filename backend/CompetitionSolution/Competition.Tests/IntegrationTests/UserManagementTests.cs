using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Competition.Application.Admin.Users;
using Competition.Application.Auth.Interfaces;
using Competition.Application.Common.Models;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Competition.Tests.IntegrationTests;

public class UserManagementTests(WebApplicationFactory<Program> factory) : BaseIntegrationTest(factory)
{
    private async Task<ApplicationUser> NewUser(string email, string role = "User", Instant? registered = null)
    {
        using var scope = ServiceProvider.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            UserName = email, Email = email, FirstName = "Account", LastName = "Test", RegisteredAt = registered,
        };
        (await manager.CreateAsync(user, "Test!aA123")).Succeeded.Should().BeTrue();
        (await manager.AddToRoleAsync(user, role)).Succeeded.Should().BeTrue();
        return user;
    }

    private void Authenticate(ApplicationUser user, string role = "Admin")
    {
        using var scope = ServiceProvider.CreateScope();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt.GenerateJwtToken(user, [role]));
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("Moderator", HttpStatusCode.Forbidden)]
    [InlineData("Manager", HttpStatusCode.Forbidden)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    public async Task AllEndpoints_RequireAdmin(string? role, HttpStatusCode expected)
    {
        if (role is not null) Authenticate(new ApplicationUser { Id = Guid.NewGuid(), Email = "caller@example.invalid" }, role);
        (await Client.GetAsync("/api/admin/users")).StatusCode.Should().Be(expected);
        (await Client.PutAsJsonAsync("/api/admin/users/" + new string('a', 32) + "/role", new { role = "User" })).StatusCode.Should().Be(expected);
        (await Client.PutAsJsonAsync("/api/admin/users/" + new string('a', 32) + "/active", new { isActive = false })).StatusCode.Should().Be(expected);
    }

    [Fact]
    public async Task List_UsesPublicIdsMarksSelf_AndFiltersBeforePaging()
    {
        var admin = await NewUser("admin@example.invalid", "Admin", Instant.FromUtc(2020, 1, 1, 0, 0));
        var target = await NewUser("target@example.invalid", "Moderator", Instant.FromUtc(2025, 5, 1, 0, 0));
        await NewUser("later@example.invalid", "User", Instant.FromUtc(2026, 1, 1, 0, 0));
        Authenticate(admin);
        var response = await Client.GetAsync("/api/admin/users?pageSize=1");
        var page = (await response.Content.ReadFromJsonAsync<PagedResult<UserListDto>>())!;
        page.TotalCount.Should().Be(3);
        page.Items.Should().ContainSingle().Which.IsSelf.Should().BeTrue();
        page.Items.Single().PublicId.Should().Be(admin.PublicId);
        (await response.Content.ReadAsStringAsync()).Should().NotContain(admin.Id.ToString()).And.NotContain("passwordHash").And.NotContain("securityStamp");
        var filters = new Dictionary<string, string>
        {
            ["search=TARGET"] = target.PublicId,
            ["role=Moderator"] = target.PublicId,
            ["registeredFrom=2025-01-01T00:00:00Z&registeredTo=2025-12-31T23:59:59Z"] = target.PublicId,
        };
        foreach (var (filter, expected) in filters)
        {
            var filtered = (await Client.GetFromJsonAsync<PagedResult<UserListDto>>($"/api/admin/users?{filter}&isActive=true&pageSize=1"))!;
            filtered.TotalCount.Should().Be(1);
            filtered.Items.Single().PublicId.Should().Be(expected);
        }
    }

    [Fact]
    public async Task RoleChange_ReplacesMemberships_AndRevokesRefreshTokens()
    {
        var admin = await NewUser("admin@example.invalid", "Admin");
        var target = await NewUser("target@example.invalid", "Admin");
        string oldAccess;
        using (var scope = ServiceProvider.CreateScope())
        {
            oldAccess = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>().GenerateJwtToken(target, ["Admin"]);
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.RefreshTokens.Add(new RefreshToken { UserId = target.Id, Token = "old-session", Expires = DateTime.UtcNow.AddDays(1) });
            await db.SaveChangesAsync();
        }
        Authenticate(admin);
        (await Client.PutAsJsonAsync($"/api/admin/users/{target.PublicId}/role", new { role = "Moderator" })).StatusCode.Should().Be(HttpStatusCode.OK);
        using var verification = ServiceProvider.CreateScope();
        var manager = verification.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var saved = (await manager.FindByIdAsync(target.Id.ToString()))!;
        (await manager.GetRolesAsync(saved)).Should().Equal("Moderator");
        var context = verification.ServiceProvider.GetRequiredService<AppDbContext>();
        var token = await context.RefreshTokens.SingleAsync();
        token.IsRevoked.Should().BeTrue();
        token.RevokedAt.Should().NotBeNull();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", oldAccess);
        (await Client.GetAsync("/api/admin/users")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeactivateAndReactivate_PreserveAccount_AndControlLogin()
    {
        var admin = await NewUser("admin@example.invalid", "Admin");
        var target = await NewUser("target@example.invalid");
        Authenticate(admin);
        (await Client.PutAsJsonAsync($"/api/admin/users/{target.PublicId}/active", new { isActive = false })).StatusCode.Should().Be(HttpStatusCode.OK);
        Authenticate(target, "User");
        (await Client.GetAsync("/api/admin/users")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        Authenticate(admin);
        (await Client.PostAsJsonAsync("/api/auth/login", new { email = target.Email, password = "Test!aA123" })).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var inactive = (await Client.GetFromJsonAsync<PagedResult<UserListDto>>("/api/admin/users?isActive=false"))!;
        inactive.Items.Should().ContainSingle().Which.PublicId.Should().Be(target.PublicId);
        (await Client.PutAsJsonAsync($"/api/admin/users/{target.PublicId}/active", new { isActive = true })).StatusCode.Should().Be(HttpStatusCode.OK);
        (await Client.PostAsJsonAsync("/api/auth/login", new { email = target.Email, password = "Test!aA123" })).StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = ServiceProvider.CreateScope();
        (await scope.ServiceProvider.GetRequiredService<AppDbContext>().Users.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task SelfChangesInvalidRolesAndMissingStatus_AreRejected()
    {
        var admin = await NewUser("admin@example.invalid", "Admin");
        var target = await NewUser("target@example.invalid");
        Authenticate(admin);
        (await Client.PutAsJsonAsync($"/api/admin/users/{admin.PublicId}/role", new { role = "User" })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.PutAsJsonAsync($"/api/admin/users/{admin.PublicId}/active", new { isActive = false })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.PutAsJsonAsync($"/api/admin/users/{target.PublicId}/role", new { role = "Unknown" })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.PutAsJsonAsync($"/api/admin/users/{target.PublicId}/active", new { })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.GetAsync("/api/admin/users?page=0")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.GetAsync("/api/admin/users?registeredFrom=2026-01-01T00:00:00Z&registeredTo=2025-01-01T00:00:00Z")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LegacyUnknownRegistrationTime_IsNotFabricatedOrIncludedInDateFilters()
    {
        var admin = await NewUser("admin@example.invalid", "Admin");
        var legacy = await NewUser("legacy@example.invalid");
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.Users.SingleAsync(x => x.Id == legacy.Id)).RegisteredAt = null;
            await db.SaveChangesAsync();
        }
        Authenticate(admin);
        var all = (await Client.GetFromJsonAsync<PagedResult<UserListDto>>("/api/admin/users?search=legacy"))!;
        all.Items.Single().RegisteredAt.Should().BeNull();
        var dated = (await Client.GetFromJsonAsync<PagedResult<UserListDto>>("/api/admin/users?search=legacy&registeredFrom=2000-01-01T00:00:00Z"))!;
        dated.TotalCount.Should().Be(0);
    }
}
