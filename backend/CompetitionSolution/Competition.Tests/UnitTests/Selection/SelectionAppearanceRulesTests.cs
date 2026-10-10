using Competition.Application.Helpers;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace Competition.Tests.UnitTests.SelectionTests;

public class SelectionAppearanceRulesTests
{
    private static readonly LocalDate MatchDay = new(2026, 6, 10);

    [Fact]
    public void CoversDate_IncludesOpenEndedMandate()
    {
        SelectionAppearanceRules.CoversDate(MatchDay, new LocalDate(2026, 1, 1), null).Should().BeTrue();
    }

    [Fact]
    public void CoversDate_RejectsDateBeforeStartOrAfterEnd()
    {
        SelectionAppearanceRules.CoversDate(MatchDay, new LocalDate(2026, 7, 1), null).Should().BeFalse();
        SelectionAppearanceRules.CoversDate(MatchDay, new LocalDate(2026, 1, 1), new LocalDate(2026, 6, 1)).Should().BeFalse();
    }

    [Fact]
    public void CoversDate_RequiresAStart()
    {
        SelectionAppearanceRules.CoversDate(MatchDay, null, null).Should().BeFalse();
    }

    [Fact]
    public void Side_ReturnsHomeAwayOrNone()
    {
        var home = Guid.NewGuid();
        var away = Guid.NewGuid();

        SelectionAppearanceRules.Side(home, home, away).Should().Be("home");
        SelectionAppearanceRules.Side(away, home, away).Should().Be("away");
        SelectionAppearanceRules.Side(Guid.NewGuid(), home, away).Should().BeNull();
    }
}
