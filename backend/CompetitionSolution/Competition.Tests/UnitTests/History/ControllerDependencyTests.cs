using System.Reflection;
using Competition.Api.Controllers;
using FluentAssertions;
using MediatR;
using Xunit;

namespace Competition.Tests.UnitTests.History;

public class ControllerDependencyTests
{
    [Fact]
    public void EveryControllerDependsOnlyOnMediator()
    {
        var controllers = typeof(PlayerController).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.Name.EndsWith("Controller"))
            .ToList();

        controllers.Should().NotBeEmpty();
        foreach (var controller in controllers)
        {
            var parameters = controller.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .SelectMany(constructor => constructor.GetParameters())
                .ToList();

            parameters.Should().NotBeEmpty();
            parameters.Should().OnlyContain(
                parameter => parameter.ParameterType == typeof(IMediator),
                $"{controller.Name} should take only IMediator");
        }
    }
}
