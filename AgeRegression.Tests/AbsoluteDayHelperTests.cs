using AgeRegression.Utilities;
using FluentAssertions;

namespace AgeRegression.Tests;

public sealed class AbsoluteDayHelperTests
{
    [Fact]
    public void ToAbsoluteDay_Year1Spring1_ReturnsZero()
    {
        AbsoluteDayHelper.ToAbsoluteDay(1, 0, 1).Should().Be(0);
    }

    [Fact]
    public void ToAbsoluteDay_Year1Summer1_Returns28()
    {
        AbsoluteDayHelper.ToAbsoluteDay(1, 1, 1).Should().Be(28);
    }

    [Fact]
    public void ToAbsoluteDay_Year2Spring1_Returns112()
    {
        AbsoluteDayHelper.ToAbsoluteDay(2, 0, 1).Should().Be(112);
    }

    [Fact]
    public void ToAbsoluteDay_IsMonotonicallyIncreasing()
    {
        var days = new List<int>();
        for (var year = 1; year <= 3; year++)
        for (var season = 0; season < 4; season++)
        for (var day = 1; day <= 28; day++)
            days.Add(AbsoluteDayHelper.ToAbsoluteDay(year, season, day));
        days.Should().BeInAscendingOrder();
    }

    [Fact]
    public void ToAbsoluteDay_SeasonBoundaryIsOneApart()
    {
        var lastSpring  = AbsoluteDayHelper.ToAbsoluteDay(1, 0, 28);
        var firstSummer = AbsoluteDayHelper.ToAbsoluteDay(1, 1, 1);
        (firstSummer - lastSpring).Should().Be(1);
    }

    [Fact]
    public void ToAbsoluteDay_YearBoundaryIsOneApart()
    {
        var lastYear1  = AbsoluteDayHelper.ToAbsoluteDay(1, 3, 28);
        var firstYear2 = AbsoluteDayHelper.ToAbsoluteDay(2, 0, 1);
        (firstYear2 - lastYear1).Should().Be(1);
    }

    [Fact]
    public void DaysBetween_SameDayReturnsZero()
    {
        AbsoluteDayHelper.DaysBetween(10, 10).Should().Be(0);
    }

    [Fact]
    public void DaysBetween_ReturnsCorrectDifference()
    {
        AbsoluteDayHelper.DaysBetween(5, 12).Should().Be(7);
    }

    [Fact]
    public void DaysBetween_NeverReturnsNegative()
    {
        AbsoluteDayHelper.DaysBetween(20, 10).Should().Be(0);
    }

    [Fact]
    public void CooldownScenario_AcrossSeasonBoundary()
    {
        var shownDay  = AbsoluteDayHelper.ToAbsoluteDay(1, 0, 27);
        var checkDay1 = AbsoluteDayHelper.ToAbsoluteDay(1, 1, 1);
        var checkDay2 = AbsoluteDayHelper.ToAbsoluteDay(1, 1, 2);
        AbsoluteDayHelper.DaysBetween(shownDay, checkDay1).Should().Be(2);
        AbsoluteDayHelper.DaysBetween(shownDay, checkDay2).Should().Be(3);
    }

    [Theory]
    [InlineData(0,  0, 1)]
    [InlineData(1, -1, 1)]
    [InlineData(1,  4, 1)]
    [InlineData(1,  0, 0)]
    [InlineData(1,  0, 29)]
    public void ToAbsoluteDay_ThrowsOnInvalidArguments(
        int year, int season, int day)
    {
        var act = () => AbsoluteDayHelper.ToAbsoluteDay(year, season, day);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
