using System.Reflection;
using Competition.Api.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace Competition.Tests.UnitTests.Geo;

/// <summary>RS1 rule: write endpoints check authorization. Admin/Moderator create and update, only Admin deletes.</summary>
public class GeoControllerAuthorizationTests
{
    [Theory]
    [InlineData(typeof(CountryController), nameof(CountryController.CreateCountry), "Admin,Moderator")]
    [InlineData(typeof(CountryController), nameof(CountryController.UpdateCountry), "Admin,Moderator")]
    [InlineData(typeof(CountryController), nameof(CountryController.DeleteCountry), "Admin")]
    [InlineData(typeof(CityController), nameof(CityController.CreateCity), "Admin,Moderator")]
    [InlineData(typeof(CityController), nameof(CityController.UpdateCity), "Admin,Moderator")]
    [InlineData(typeof(CityController), nameof(CityController.DeleteCity), "Admin")]
    [InlineData(typeof(StadiumController), nameof(StadiumController.CreateStadium), "Admin,Moderator")]
    [InlineData(typeof(StadiumController), nameof(StadiumController.UpdateStadium), "Admin,Moderator")]
    [InlineData(typeof(StadiumController), nameof(StadiumController.DeleteStadium), "Admin")]
    public void WriteEndpoint_ShouldRequireExpectedRoles(Type controller, string action, string expectedRoles)
    {
        // Act
        var attribute = controller.GetMethod(action)!.GetCustomAttribute<AuthorizeAttribute>();

        // Assert
        attribute.Should().NotBeNull();
        attribute!.Roles.Should().Be(expectedRoles);
    }
}
