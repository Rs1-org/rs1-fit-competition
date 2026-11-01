using Competition.Application.Commands.ManagerCommands.Delete;
using Competition.Application.Commands.PlayerCommands.Delete;
using Competition.Application.Commands.RefereeCommands.Delete;
using FluentAssertions;
using Xunit;

namespace Competition.Tests.UnitTests.Person;

public class DeletePersonCommandValidatorTests
{
    [Fact]
    public void DeletePlayer_EmptyId_ShouldFail()
    {
        var result = new DeletePlayerCommandValidator().Validate(new DeletePlayerCommand(""));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DeletePlayerCommand.PublicId));
    }

    [Fact]
    public void DeleteManager_EmptyId_ShouldFail()
    {
        var result = new DeleteManagerCommandValidator().Validate(new DeleteManagerCommand(""));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void DeleteReferee_KnownId_ShouldPass()
    {
        var result = new DeleteRefereeCommandValidator().Validate(new DeleteRefereeCommand(Guid.NewGuid().ToString("N")));
        result.IsValid.Should().BeTrue();
    }
}
