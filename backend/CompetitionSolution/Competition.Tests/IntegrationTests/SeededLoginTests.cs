using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Infrastructure.Settings;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Identity;

namespace Competition.Tests.IntegrationTests;

public class SeededLoginTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _password = $"Test!aA1{Guid.NewGuid():N}";

    public SeededLoginTests(WebApplicationFactory<Program> factory)
    {
        var name = $"SeedTest_{Guid.NewGuid()}";
        _factory = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(name));
            services.PostConfigure<SeedSettings>(s =>
            {
                s.Enabled = true;
                s.AdminPassword = s.ModeratorPassword = s.ManagerPassword = s.UserPassword = _password;
            });
        }));
        _client = _factory.CreateClient();
    }

    [Theory]
    [InlineData("Admin", true, true)]
    [InlineData("Moderator", true, false)]
    [InlineData("Manager", false, false)]
    [InlineData("User", false, false)]
    public async Task SeededAccount_CanLogin_WithOnlyItsOwnRole_AndRespectsAuthorization(
        string role, bool canCreate, bool canDelete)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = $"{role.ToLowerInvariant()}@competition.ba", password = _password,
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.GetProperty("roles").EnumerateArray().Select(x => x.GetString())
            .Should().Equal(role);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            json.RootElement.GetProperty("accessToken").GetString());

        // Invalid data reaches the validator only when the caller has permission.
        var create = await _client.PostAsJsonAsync("/api/club/create", new { name = "" });
        create.StatusCode.Should().Be(canCreate ? HttpStatusCode.BadRequest : HttpStatusCode.Forbidden);
        var delete = await _client.DeleteAsync("/api/club/delete/" + new string('a', 32));
        delete.StatusCode.Should().Be(canDelete ? HttpStatusCode.NotFound : HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RepeatedSeeding_PreservesPasswordsActiveFlagsAndMemberships()
    {
        using var scope = _factory.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var admin = (await manager.FindByEmailAsync("admin@competition.ba"))!;
        var changedPassword = $"Changed!aA1{Guid.NewGuid():N}";
        (await manager.ChangePasswordAsync(admin, _password, changedPassword)).Succeeded.Should().BeTrue();
        admin.IsActive = false;
        await manager.UpdateAsync(admin);
        var hash = admin.PasswordHash;
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        (await db.Users.CountAsync()).Should().Be(4);
        (await db.Roles.CountAsync()).Should().Be(4);
        (await db.UserRoles.CountAsync()).Should().Be(4);
        admin.PasswordHash.Should().Be(hash);
        admin.IsActive.Should().BeFalse();
        (await manager.CheckPasswordAsync(admin, changedPassword)).Should().BeTrue();
    }

    [Fact]
    public async Task DisabledSeeding_CreatesRolesButNoAccounts()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.RemoveRange(db.Users);
        await db.SaveChangesAsync();
        var seeder = NewSeeder(scope.ServiceProvider, new SeedSettings());

        await seeder.SeedAsync();

        (await db.Users.CountAsync()).Should().Be(0);
        (await db.Roles.CountAsync()).Should().Be(4);
    }

    [Fact]
    public async Task InvalidPassword_FailsBeforeCreatingAnyAccounts_WithoutLeakingPassword()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.RemoveRange(db.Users);
        await db.SaveChangesAsync();
        var invalid = "weak-test-value";
        var seeder = NewSeeder(scope.ServiceProvider, new SeedSettings
        {
            Enabled = true, AdminPassword = _password, ModeratorPassword = _password,
            UserPassword = _password, ManagerPassword = invalid,
        });

        var act = () => seeder.SeedAsync();
        var error = await act.Should().ThrowAsync<InvalidOperationException>();
        error.Which.Message.Should().NotContain(invalid);
        (await db.Users.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ExistingSeedEmail_DoesNotGetElevatedToAdmin()
    {
        using var scope = _factory.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = (await manager.FindByEmailAsync("admin@competition.ba"))!;
        await manager.RemoveFromRoleAsync(admin, "Admin");
        await manager.AddToRoleAsync(admin, "User");
        var act = () => scope.ServiceProvider.GetRequiredService<DataSeeder>().SeedAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already used*");

        (await manager.GetRolesAsync(admin)).Should().Equal("User");
    }

    private static DataSeeder NewSeeder(IServiceProvider services, SeedSettings settings) => new(
        services.GetRequiredService<RoleManager<ApplicationRole>>(),
        services.GetRequiredService<UserManager<ApplicationUser>>(),
        Microsoft.Extensions.Options.Options.Create(settings),
        services.GetRequiredService<AppDbContext>());

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
