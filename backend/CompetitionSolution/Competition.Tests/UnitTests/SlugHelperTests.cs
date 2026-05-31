using Competition.Application.Helpers;
using FluentAssertions;
using Xunit;

namespace Competition.Tests.UnitTests;

public class SlugHelperTests
{
    [Theory]
    [InlineData("Hello World", "hello-world")]
    [InlineData("Test With Special #@! Characters", "test-with-special-characters")]
    [InlineData("Multiple   Spaces", "multiple-spaces")]
    [InlineData("Already-A-Slug", "already-a-slug")]
    [InlineData("ČĆŽŠĐ", "cczsd")]
    [InlineData("  Trim Me  ", "trim-me")]
    public void GenerateSlug_ShouldReturnCorrectSlug(string input, string expected)
    {
        // Act
        var result = SlugHelper.GenerateSlug(input);

        // Assert
        result.Should().Be(expected);
    }
}
