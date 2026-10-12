using Competition.Infrastructure.Settings;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace Competition.Tests.IntegrationTests;

public class ConfigurationTests : BaseIntegrationTest
{
    public ConfigurationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public void App_ShouldRunInDevelopment_WithSettingsFromDevelopmentFile()
    {
        // Arrange
        var environment = ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var jwt = ServiceProvider.GetRequiredService<IOptions<JwtSettings>>().Value;
        var cors = ServiceProvider.GetRequiredService<IOptions<CorsSettings>>().Value;

        // Assert
        environment.IsDevelopment().Should().BeTrue();
        jwt.Issuer.Should().Be("CompetitionApi.Development");
        jwt.Key.Length.Should().BeGreaterThanOrEqualTo(32);
        cors.AllowedOrigins.Should().Contain("http://localhost:4200");
    }

    [Fact]
    public async Task CorsPreflight_FromAllowedOrigin_ShouldReturnAllowOriginHeader()
    {
        // Arrange
        var request = BuildPreflight("http://localhost:4200");

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values).Should().BeTrue();
        values.Should().ContainSingle().Which.Should().Be("http://localhost:4200");
    }

    [Fact]
    public async Task CorsPreflight_FromUnknownOrigin_ShouldNotReturnAllowOriginHeader()
    {
        // Arrange
        var request = BuildPreflight("https://evil.example.com");

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse();
    }

    private static HttpRequestMessage BuildPreflight(string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/City/getCities");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");
        return request;
    }
}
