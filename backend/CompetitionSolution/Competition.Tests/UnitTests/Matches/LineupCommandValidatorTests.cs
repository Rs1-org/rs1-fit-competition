using Competition.Application.Commands.MatchLineupCommands;
using Competition.Application.DTOs.MatchDto;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Competition.Tests.UnitTests.Matches;

public class LineupCommandValidatorTests
{
    private static string Id() => Guid.NewGuid().ToString("N");

    private static SaveLineupPlayerDto Player(string? player = null, string? selection = null, int? shirt = 7, bool starter = false) => new()
    {
        PlayerPublicId = player ?? Id(),
        SelectionPublicId = selection ?? Id(),
        IsStarter = starter,
        ShirtNumber = shirt
    };

    [Fact]
    public async Task Save_ShouldRejectDuplicatePlayerRepeatedShirtAndTooManyStarters()
    {
        var validator = new SaveMatchLineupCommandValidator(new AlwaysFoundLookup());
        var selection = Id();
        var player = Id();

        var duplicate = await validator.ValidateAsync(new SaveMatchLineupCommand
        {
            PublicId = Id(),
            Players = [Player(player, selection), Player(player, selection, 8)]
        });
        duplicate.IsValid.Should().BeFalse();
        duplicate.Errors.Should().Contain(e => e.ErrorMessage.Contains("only once"));

        var shirt = await validator.ValidateAsync(new SaveMatchLineupCommand
        {
            PublicId = Id(),
            Players = [Player(shirt: 0), Player(shirt: 100)]
        });
        shirt.IsValid.Should().BeFalse();

        var repeatedShirt = await validator.ValidateAsync(new SaveMatchLineupCommand
        {
            PublicId = Id(),
            Players = [Player(selection: selection, shirt: 9), Player(selection: selection, shirt: 9)]
        });
        repeatedShirt.Errors.Should().Contain(e => e.ErrorMessage.Contains("unique"));

        var starters = Enumerable.Range(1, 12)
            .Select(n => Player(selection: selection, shirt: n, starter: true))
            .ToList();
        var tooMany = await validator.ValidateAsync(new SaveMatchLineupCommand { PublicId = Id(), Players = starters });
        tooMany.Errors.Should().Contain(e => e.ErrorMessage.Contains("11 starters"));
    }

    [Fact]
    public async Task Save_ShouldAcceptOnePlayerAndAnEmptyRefereeAssignment()
    {
        var lookup = new AlwaysFoundLookup();
        var lineup = await new SaveMatchLineupCommandValidator(lookup).ValidateAsync(new SaveMatchLineupCommand
        {
            PublicId = Id(),
            Players = [Player()]
        });
        lineup.IsValid.Should().BeTrue();

        var referee = await new AssignMatchRefereeCommandValidator(lookup).ValidateAsync(new AssignMatchRefereeCommand
        {
            PublicId = Id(),
            RefereePublicId = null
        });
        referee.IsValid.Should().BeTrue();
    }
}
