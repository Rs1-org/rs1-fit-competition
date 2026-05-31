using System.Net;
using System.Net.Http.Json;
using Competition.Application.Common.Models;
using Competition.Application.DTOs.CityDto;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Competition.Tests.IntegrationTests;

public class CityControllerTests : BaseIntegrationTest
{
    public CityControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetCities_ShouldReturnOk()
    {
        // Act
        var response = await Client.GetAsync("/api/City/getCities");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PagedResult<CityListDto>>();
        result.Should().NotBeNull();
    }
}
