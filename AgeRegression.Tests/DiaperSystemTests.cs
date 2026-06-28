using AgeRegression.Data;
using AgeRegression.State;
using AgeRegression.Systems;
using AgeRegression.Utilities;
using FluentAssertions;

namespace AgeRegression.Tests;

public sealed class DiaperSystemTests
{
    [Fact]
    public void CalculateWetnessIncrease_ReferenceCapacity_ReturnsReferenceIncrease()
    {
        var diaper = new DiaperTypeData { Id = "test", MaxCapacity = 100f, AbsorptionRate = 1f };
        DiaperCalculations.CalculateWetnessIncrease(diaper)
            .Should().BeApproximately(0.20f, 0.001f);
    }

    [Fact]
    public void CalculateWetnessIncrease_HigherCapacity_ReturnsLowerIncrease()
    {
        var standard = new DiaperTypeData { Id = "s", MaxCapacity = 100f, AbsorptionRate = 1f };
        var premium  = new DiaperTypeData { Id = "p", MaxCapacity = 200f, AbsorptionRate = 1f };
        DiaperCalculations.CalculateWetnessIncrease(premium)
            .Should().BeLessThan(
                DiaperCalculations.CalculateWetnessIncrease(standard));
    }

    [Fact]
    public void CalculateWetnessIncrease_HighAbsorption_FillsSlower()
    {
        var standard = new DiaperTypeData
            { Id = "s", MaxCapacity = 100f, AbsorptionRate = 1.0f };
        var highAbs  = new DiaperTypeData
            { Id = "h", MaxCapacity = 100f, AbsorptionRate = 1.5f };
        DiaperCalculations.CalculateWetnessIncrease(highAbs)
            .Should().BeLessThan(
                DiaperCalculations.CalculateWetnessIncrease(standard));
    }

    [Fact]
    public void MinutesBetween_ThirtyMinutes_Returns30()
    {
        GameTimeHelper.MinutesBetween(600, 630).Should().Be(30);
    }

    [Fact]
    public void MinutesBetween_AcrossHourBoundary_IsCorrect()
    {
        GameTimeHelper.MinutesBetween(650, 710).Should().Be(20);
    }

    [Fact]
    public void MinutesBetween_ToTimeBeforeFromTime_ReturnsZero()
    {
        GameTimeHelper.MinutesBetween(900, 800).Should().Be(0);
    }
}
