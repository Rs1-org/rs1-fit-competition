using Competition.Application.Commands.CompetitionCommands.Create;
using Competition.Application.Commands.CompetitionCommands.Validators;
using Competition.Application.Commands.RoundCommands.Create;
using Competition.Application.Commands.RoundCommands.Validators;
using Competition.Application.Commands.SeasonCommands.Create;
using Competition.Application.Commands.SeasonCommands.Validators;
using FluentAssertions;
using Xunit;

namespace Competition.Tests.UnitTests.Competitions;

public class CompetitionValidatorTests
{
    [Theory]
    [InlineData("")]
    [InlineData("A")]
    public void CreateCompetition_ShortOrEmptyName_ShouldFail(string name)
    {
        var result = new CreateCompetitionCommandValidator().Validate(new CreateCompetitionCommand { Name = name });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCompetitionCommand.Name));
    }

    [Fact]
    public void CreateCompetition_ValidCommand_ShouldPass()
    {
        var result = new CreateCompetitionCommandValidator().Validate(new CreateCompetitionCommand
        {
            Name = "Premier League",
            ShortName = "EPL"
        });

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("2025-08-01", "2025-08-01")]
    [InlineData("2026-05-31", "2025-08-01")]
    public void CreateSeason_EndNotAfterStart_ShouldFail(string start, string end)
    {
        var result = new CreateSeasonCommandValidator().Validate(new CreateSeasonCommand
        {
            CompetitionPublicId = Competition.Domain.Entities.BaseEntity.NewPublicId(),
            Name = "2025/26",
            StartDate = DateOnly.Parse(start),
            EndDate = DateOnly.Parse(end)
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateSeasonCommand.EndDate));
    }

    [Fact]
    public void CreateSeason_MissingCompetition_ShouldFail()
    {
        var result = new CreateSeasonCommandValidator().Validate(new CreateSeasonCommand
        {
            Name = "2025/26",
            StartDate = new DateOnly(2025, 8, 1),
            EndDate = new DateOnly(2026, 5, 31)
        });

        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateSeasonCommand.CompetitionPublicId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void CreateRound_NumberOutOfRange_ShouldFail(int number)
    {
        var result = new CreateRoundCommandValidator().Validate(new CreateRoundCommand
        {
            SeasonPublicId = Competition.Domain.Entities.BaseEntity.NewPublicId(),
            Name = "Round",
            Number = number
        });

        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateRoundCommand.Number));
    }
}
