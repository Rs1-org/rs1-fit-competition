using System.Net;
using System.Text.Json;
using Competition.Domain.Entities;
using Competition.Infrastructure.Data;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Competition.Tests.IntegrationTests;

/// <summary>RS1 item 135: Muhamed's endpoints expose PublicId, never the internal Id.</summary>
public class PublicIdApiTests : BaseIntegrationTest
{
    public PublicIdApiTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task CountryList_ShouldReturnPublicIdAndNoInternalId()
    {
        // Arrange
        var country = new Country { Name = "Bosnia and Herzegovina", Slug = "bosnia-and-herzegovina" };
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Countries.Add(country);
            await db.SaveChangesAsync();
        }

        // Act
        var response = await Client.GetAsync("/api/Country/getAll");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var item = json.RootElement.GetProperty("items")[0];
        item.GetProperty("publicId").GetString().Should().Be(country.PublicId);
        item.TryGetProperty("id", out _).Should().BeFalse("the internal Guid must not leave the API");
    }

    [Theory]
    [InlineData("/api/Match/getByPublicId/ffffffffffffffffffffffffffffffff")]
    [InlineData("/api/City/getCitiesByCountry/ffffffffffffffffffffffffffffffff")]
    public async Task UnknownPublicId_ShouldReturn404(string url)
    {
        // Act
        var response = await Client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MalformedPublicId_ShouldReturn400FromValidation()
    {
        // Act
        var response = await Client.GetAsync("/api/City/getCitiesByCountry/not-a-public-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
