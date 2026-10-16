using Competition.Application.Commands.MatchCommands.Create;
using Competition.Application.Commands.MatchCommands.Update;
using Competition.Application.Commands.MatchCommands.Validators;
using FluentAssertions;
using Xunit;

namespace Competition.Tests.UnitTests.Matches;

/// <summary>COMP-77: business rules in the Match create/update validators.</summary>
public class MatchValidatorTests
{
    private static string NewPublicId() => Guid.NewGuid().ToString("N");

    private static CreateMatchCommand ValidCreate() => new()
    {
        Slug = "zeljeznicar-sarajevo",
        Date = DateTime.UtcNow.AddDays(7),
        StatusPublicId = NewPublicId(),
        StadiumPublicId = NewPublicId(),
        HomeSelectionPublicId = NewPublicId(),
        AwaySelectionPublicId = NewPublicId(),
        FollowersCount = 0
    };

    private static UpdateMatchCommand ValidUpdate() => new()
    {
        Slug = "zeljeznicar-sarajevo",
        Date = DateTime.UtcNow.AddDays(-1),
        StatusPublicId = NewPublicId(),
        StadiumPublicId = NewPublicId(),
        HomeScore = 2,
        AwayScore = 1,
        HalfHomeScore = 1,
        HalfAwayScore = 0
    };

    // ---------- Create ----------

    [Fact]
    public void Create_ValidCommand_ShouldPass()
    {
        new CreateMatchCommandValidator().Validate(ValidCreate()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Create_SameHomeAndAwaySelection_ShouldFail()
    {
        var command = ValidCreate();
        command.AwaySelectionPublicId = command.HomeSelectionPublicId;

        var result = new CreateMatchCommandValidator().Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Home and away selection must be different.");
    }

    [Fact]
    public void Create_NoSelectionsYet_ShouldPass()
    {
        var command = ValidCreate();
        command.HomeSelectionPublicId = null;
        command.AwaySelectionPublicId = null;

        new CreateMatchCommandValidator().Validate(command).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Create_MissingStatus_ShouldFail()
    {
        var command = ValidCreate();
        command.StatusPublicId = "";

        var result = new CreateMatchCommandValidator().Validate(command);

        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateMatchCommand.StatusPublicId));
    }

    [Fact]
    public void Create_MissingStadium_ShouldFail()
    {
        var command = ValidCreate();
        command.StadiumPublicId = "";

        var result = new CreateMatchCommandValidator().Validate(command);

        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateMatchCommand.StadiumPublicId));
    }

    [Fact]
    public void Create_DateInThePast_ShouldFail()
    {
        var command = ValidCreate();
        command.Date = DateTime.UtcNow.AddHours(-1);

        var result = new CreateMatchCommandValidator().Validate(command);

        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateMatchCommand.Date));
    }

    [Fact]
    public void Create_NegativeFollowers_ShouldFail()
    {
        var command = ValidCreate();
        command.FollowersCount = -1;

        new CreateMatchCommandValidator().Validate(command).IsValid.Should().BeFalse();
    }

    // ---------- Update ----------

    [Fact]
    public void Update_ValidPastMatchWithScores_ShouldPass()
    {
        new UpdateMatchCommandValidator().Validate(ValidUpdate()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Update_HalfTimeScoreAboveFullTime_ShouldFail()
    {
        var command = ValidUpdate();
        command.HalfHomeScore = 3;

        var result = new UpdateMatchCommandValidator().Validate(command);

        result.Errors.Should().Contain(e => e.ErrorMessage == "Half-time home score cannot exceed full-time home score.");
    }

    [Fact]
    public void Update_OnlyOneScoreSet_ShouldFail()
    {
        var command = ValidUpdate();
        command.AwayScore = null;
        command.HalfHomeScore = null;
        command.HalfAwayScore = null;

        var result = new UpdateMatchCommandValidator().Validate(command);

        result.Errors.Should().Contain(e => e.ErrorMessage == "HomeScore and AwayScore must both be set or both be null.");
    }

    [Fact]
    public void Update_SameHomeAndAwaySelection_ShouldFail()
    {
        var command = ValidUpdate();
        command.HomeSelectionPublicId = command.AwaySelectionPublicId = NewPublicId();

        new UpdateMatchCommandValidator().Validate(command).IsValid.Should().BeFalse();
    }
}
