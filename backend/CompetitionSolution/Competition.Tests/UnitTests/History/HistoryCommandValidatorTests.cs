using Competition.Application.Commands.MandateDateCommands;
using Competition.Application.Commands.PlayerSelectionHistoryCommands;
using Competition.Application.Commands.SelectionManagerHistoryCommands;
using Competition.Application.Commands.TransferValueHistoryCommands;
using FluentAssertions;
using Xunit;

namespace Competition.Tests.UnitTests.History;

public class HistoryCommandValidatorTests
{
    [Fact]
    public void Transfer_RejectsANegativeValue()
    {
        var result = new CreateTransferValueHistoryCommandValidator()
            .Validate(new CreateTransferValueHistoryCommand("2026-10-15", -1, Guid.NewGuid()));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "Value");
    }

    [Fact]
    public void Transfer_AcceptsAnEmptyDateAndANonNegativeValue()
    {
        var result = new CreateTransferValueHistoryCommandValidator()
            .Validate(new CreateTransferValueHistoryCommand(null, 12.50m, Guid.NewGuid()));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void PlayerSelection_RejectsAShirtNumberOutsideTheSquad()
    {
        var result = new CreatePlayerSelectionHistoryCommandValidator()
            .Validate(new CreatePlayerSelectionHistoryCommand(1, 1, 0, 100, Guid.NewGuid(), null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "ShirtNumber");
    }

    [Fact]
    public void ManagerHistory_RejectsNotesThatAreTooLong()
    {
        var result = new CreateSelectionManagerHistoryCommandValidator()
            .Validate(new CreateSelectionManagerHistoryCommand(new string('a', 2001), 1, 0, 0, Guid.NewGuid(), null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "Notes");
    }

    [Fact]
    public void Mandate_RejectsAnEndDateBeforeTheStart()
    {
        var result = new CreateMandateDateCommandValidator()
            .Validate(new CreateMandateDateCommand("2026-10-15", "2026-01-01", null, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.ErrorMessage == "End date must be on or after the start date.");
    }

    [Fact]
    public void Delete_RejectsAnEmptyId()
    {
        var result = new DeleteTransferValueHistoryCommandValidator()
            .Validate(new DeleteTransferValueHistoryCommand(Guid.Empty));

        result.IsValid.Should().BeFalse();
    }
}
