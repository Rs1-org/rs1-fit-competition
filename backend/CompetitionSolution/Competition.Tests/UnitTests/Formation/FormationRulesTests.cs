using Competition.Application.Helpers;
using FluentAssertions;
using Xunit;

namespace Competition.Tests.UnitTests.FormationTests;

public class FormationRulesTests
{
    [Theory]
    [InlineData("4-3-3", 4, 3, 3, true)]
    [InlineData("4-2-3-1", 4, 2, 3, false)]
    [InlineData("5-4-1", 4, 4, 1, false)]
    [InlineData("", 4, 3, 3, false)]
    public void CodeMatchesMark_ComparesThreeLines(string code, int defenders, int midfielders, int attackers, bool expected)
    {
        FormationRules.CodeMatchesMark(code, defenders, midfielders, attackers).Should().Be(expected);
    }
}
