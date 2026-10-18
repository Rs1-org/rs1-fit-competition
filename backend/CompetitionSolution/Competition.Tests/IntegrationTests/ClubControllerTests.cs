using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Competition.Application.Auth.Interfaces;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.ClubDto;
using Competition.Tests.Infrastructure;
using Competition.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Competition.Tests.IntegrationTests;

public class ClubControllerTests(WebApplicationFactory<Program> factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task PublicList_ReturnsPageAndRejectsInvalidFilters()
    {
        var response = await Client.GetAsync("/api/club?page=1&pageSize=5&hasStadium=false");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<PagedResult<ClubListDto>>())!.PageSize.Should().Be(5);
        (await Client.GetAsync("/api/club?page=0")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.GetAsync("/api/club?foundedFrom=2025-02-30")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AnonymousWrites_AreUnauthorized()
    {
        (await Client.PostAsJsonAsync("/api/club/create", new { name = "Club" })).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await Client.PutAsJsonAsync("/api/club/update/club", new { name = "Club" })).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await Client.DeleteAsync("/api/club/delete/" + new string('a', 32))).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("Admin", HttpStatusCode.OK)]
    [InlineData("Moderator", HttpStatusCode.Forbidden)]
    public async Task PrivilegedRoles_CanCreateAndEdit_AndOnlyAdminCanSoftDelete(string role, HttpStatusCode deleteStatus)
    {
        using (var scope = ServiceProvider.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var email = $"club-{Guid.NewGuid():N}@example.invalid";
            var user = new ApplicationUser { Email = email, UserName = email };
            (await users.CreateAsync(user)).Succeeded.Should().BeTrue();
            (await users.AddToRoleAsync(user, role)).Succeeded.Should().BeTrue();
            var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt.GenerateJwtToken(user, await users.GetRolesAsync(user)));
        }
        var name = $"Club {Guid.NewGuid():N}";
        var created = await Client.PostAsJsonAsync("/api/club/create", new { name, foundingDate = "1921-09-19", clubAge = 7 });
        created.StatusCode.Should().Be(HttpStatusCode.OK);
        var club = (await created.Content.ReadFromJsonAsync<ClubDetailsDto>())!;
        club.PublicId.Should().HaveLength(32);
        var updated = await Client.PutAsJsonAsync($"/api/club/update/{club.Slug}", new { name = "Renamed " + name, foundingDate = "1921-09-19", clubAge = 7 });
        updated.StatusCode.Should().Be(HttpStatusCode.OK);
        var renamed = (await updated.Content.ReadFromJsonAsync<ClubDetailsDto>())!;
        renamed.PublicId.Should().Be(club.PublicId);
        (await Client.GetAsync($"/api/club/get/{club.Slug}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Client.DeleteAsync($"/api/club/delete/{club.PublicId}")).StatusCode.Should().Be(deleteStatus);
        (await Client.GetAsync($"/api/club/get/{renamed.Slug}")).StatusCode.Should().Be(role == "Admin" ? HttpStatusCode.NotFound : HttpStatusCode.OK);
    }
}
