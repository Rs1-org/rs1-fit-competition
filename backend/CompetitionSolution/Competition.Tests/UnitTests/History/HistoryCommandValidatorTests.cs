using Competition.Application.Commands.MandateDateCommands;
using Competition.Application.Commands.PlayerSelectionHistoryCommands;
using Competition.Application.Commands.SelectionManagerHistoryCommands;
using Competition.Application.Commands.TransferValueHistoryCommands;
using Competition.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Competition.Tests.UnitTests.History;

public class HistoryCommandValidatorTests
{
    private static string PublicId() => Guid.NewGuid().ToString("N");

    [Fact]
    public async Task Transfer_RejectsANegativeValue()
    {
        var result = await new CreateTransferValueHistoryCommandValidator(new AlwaysFoundLookup())
            .ValidateAsync(new CreateTransferValueHistoryCommand("2026-10-15", -1, PublicId()));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "Value");
    }

    [Fact]
    public async Task Transfer_AcceptsAnEmptyDateAndANonNegativeValue()
    {
        var result = await new CreateTransferValueHistoryCommandValidator(new AlwaysFoundLookup())
            .ValidateAsync(new CreateTransferValueHistoryCommand(null, 12.50m, PublicId()));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task PlayerSelection_RejectsAShirtNumberOutsideTheSquad()
    {
        var result = await new CreatePlayerSelectionHistoryCommandValidator(new AlwaysFoundLookup())
            .ValidateAsync(new CreatePlayerSelectionHistoryCommand(1, 1, 0, 100, PublicId(), null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "ShirtNumber");
    }

    [Fact]
    public async Task ManagerHistory_RejectsNotesThatAreTooLong()
    {
        var result = await new CreateSelectionManagerHistoryCommandValidator(new AlwaysFoundLookup())
            .ValidateAsync(new CreateSelectionManagerHistoryCommand(new string('a', 2001), 1, 0, 0, PublicId(), null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "Notes");
    }

    [Fact]
    public async Task Mandate_RejectsAnEndDateBeforeTheStart()
    {
        var result = await new CreateMandateDateCommandValidator(new AlwaysFoundLookup())
            .ValidateAsync(new CreateMandateDateCommand("2026-10-15", "2026-01-01", null, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.ErrorMessage == "End date must be on or after the start date.");
    }

    [Fact]
    public void Delete_RejectsAnEmptyId()
    {
        var result = new DeleteTransferValueHistoryCommandValidator()
            .Validate(new DeleteTransferValueHistoryCommand(""));

        result.IsValid.Should().BeFalse();
    }
}
