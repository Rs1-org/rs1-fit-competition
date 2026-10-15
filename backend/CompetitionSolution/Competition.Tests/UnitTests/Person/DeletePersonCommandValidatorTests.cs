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
        var result = new DeletePlayerCommandValidator().Validate(new DeletePlayerCommand(Guid.Empty));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DeletePlayerCommand.Id));
    }

    [Fact]
    public void DeleteManager_EmptyId_ShouldFail()
    {
        var result = new DeleteManagerCommandValidator().Validate(new DeleteManagerCommand(Guid.Empty));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void DeleteReferee_KnownId_ShouldPass()
    {
        var result = new DeleteRefereeCommandValidator().Validate(new DeleteRefereeCommand(Guid.NewGuid()));
        result.IsValid.Should().BeTrue();
    }
}
