using AgeRegression.State;
using FluentAssertions;

namespace AgeRegression.Tests;

public sealed class DiaperStateTests
{
    [Fact]
    public void None_IsNotWearingDiaper()
    {
        DiaperState.None.IsWearingDiaper.Should().BeFalse();
        DiaperState.None.EquippedDiaperTypeId.Should().BeNull();
    }

    [Fact]
    public void None_ConditionIdIsNone()
    {
        DiaperState.None.ConditionId.Should().Be("none");
    }

    [Theory]
    [InlineData(0.00f, "clean")]
    [InlineData(0.01f, "damp")]
    [InlineData(0.32f, "damp")]
    [InlineData(0.33f, "wet")]
    [InlineData(0.65f, "wet")]
    [InlineData(0.66f, "soaked")]
    [InlineData(1.00f, "soaked")]
    public void ConditionId_DerivedCorrectlyFromWetnessLevel(
        float wetness, string expected)
    {
        DiaperState.CreateFresh("basic").WithWetness(wetness).ConditionId.Should().Be(expected);
    }

    [Fact]
    public void WithWetness_ClampsAboveOne()
    {
        DiaperState.None.WithWetness(5f).WetnessLevel.Should().Be(1f);
    }

    [Fact]
    public void WithWetness_ClampsBelowZero()
    {
        DiaperState.None.WithWetness(-1f).WetnessLevel.Should().Be(0f);
    }

    [Fact]
    public void WithWetness_DoesNotMutateOriginal()
    {
        var original = DiaperState.CreateFresh("basic", false, 0);
        var modified = original.WithWetness(0.5f);
        original.WetnessLevel.Should().Be(0f);
        modified.WetnessLevel.Should().Be(0.5f);
    }

    [Fact]
    public void Equals_ReturnsTrueForStructurallyIdenticalStates()
    {
        var a = DiaperState.CreateFresh("basic", false, 5);
        var b = DiaperState.CreateFresh("basic", false, 5);
        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equals_ReturnsFalseWhenWetnessDiffers()
    {
        var a = DiaperState.CreateFresh("basic", false, 5).WithWetness(0.1f);
        var b = DiaperState.CreateFresh("basic", false, 5).WithWetness(0.2f);
        a.Equals(b).Should().BeFalse();
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Equals_NoneEqualsNone()
    {
        (DiaperState.None == DiaperState.None).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_IsConsistentForEqualStates()
    {
        var a = DiaperState.CreateFresh("basic", false, 5);
        var b = DiaperState.CreateFresh("basic", false, 5);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Equals_NullReturnsFalse()
    {
        DiaperState.CreateFresh("basic", false, 0).Equals(null).Should().BeFalse();
    }
}
