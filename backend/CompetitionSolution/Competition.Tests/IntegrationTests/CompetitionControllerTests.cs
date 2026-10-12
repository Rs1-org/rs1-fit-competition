using System.Net;
using System.Net.Http.Json;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.CompetitionDto;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Competition.Tests.IntegrationTests;

public class CompetitionControllerTests : BaseIntegrationTest
{
    public CompetitionControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAll_ShouldReturnPagedResult()
    {
        // Act
        var response = await Client.GetAsync("/api/Competition/getAll?page=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<CompetitionListDto>>();
        result.Should().NotBeNull();
        result!.PageSize.Should().Be(5);
    }

    [Theory]
    [InlineData("/api/Competition/create")]
    [InlineData("/api/Season/create")]
    [InlineData("/api/Round/create")]
    public async Task Create_WithoutToken_ShouldReturnUnauthorized(string url)
    {
        // Act
        var response = await Client.PostAsJsonAsync(url, new { name = "Anything" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
