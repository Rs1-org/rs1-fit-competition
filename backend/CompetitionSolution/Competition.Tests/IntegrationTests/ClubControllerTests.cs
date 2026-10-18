using System.Net;
using System.Net.Http.Json;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.ClubDto;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
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
}
