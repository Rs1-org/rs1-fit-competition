using Competition.Application.Queries.Reports;
using FluentAssertions;
using Xunit;

namespace Competition.Tests.UnitTests.Reports;

public class ReportValidatorTests
{
    private static readonly DateTime From = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
    private static string Id() => Guid.NewGuid().ToString("N");

    [Fact]
    public async Task MatchReport_ShouldRejectAnInvalidId()
    {
        var result = await new GetMatchReportQueryValidator().ValidateAsync(new GetMatchReportQuery("not a public id"));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task MatchReport_ShouldAcceptAValidId()
    {
        var result = await new GetMatchReportQueryValidator().ValidateAsync(new GetMatchReportQuery(Id()));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task MatchesReport_ShouldRequireBothDates()
    {
        var validator = new GetMatchesReportQueryValidator();

        (await validator.ValidateAsync(new GetMatchesReportQuery { DateFrom = From })).IsValid.Should().BeFalse();
        (await validator.ValidateAsync(new GetMatchesReportQuery { DateTo = From })).IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task MatchesReport_ShouldRejectDateFromAfterDateTo()
    {
        var result = await new GetMatchesReportQueryValidator()
            .ValidateAsync(new GetMatchesReportQuery { DateFrom = From, DateTo = From.AddDays(-1) });

        result.Errors.Should().ContainSingle().Which.PropertyName.Should().Be(nameof(GetMatchesReportQuery.DateTo));
    }

    [Fact]
    public async Task MatchesReport_ShouldAcceptASingleDayWithFilters()
    {
        var result = await new GetMatchesReportQueryValidator().ValidateAsync(new GetMatchesReportQuery
        {
            DateFrom = From,
            DateTo = From,
            StadiumPublicId = Id(),
            CompetitionPublicId = Id()
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task MatchesReport_ShouldRejectAnInvalidFilterId()
    {
        var result = await new GetMatchesReportQueryValidator().ValidateAsync(new GetMatchesReportQuery
        {
            DateFrom = From,
            DateTo = From,
            CompetitionPublicId = "not a public id"
        });

        result.IsValid.Should().BeFalse();
    }
}
