using AgeRegression.Events;
using FluentAssertions;
using Xunit;

namespace AgeRegression.Tests;

public sealed class CareSystemTests
{
    [Fact]
    public void AccessoryChangedEventArgs_HasCorrectProperties()
    {
        var args = new AccessoryChangedEventArgs("pacifier", true, 123456789);
        
        args.AccessoryId.Should().Be("pacifier");
        args.Equipped.Should().BeTrue();
        args.PlayerId.Should().Be(123456789);
    }

    [Fact]
    public void AccessoryChangedEventArgs_Unequip_ReturnsFalse()
    {
        var args = new AccessoryChangedEventArgs("mittens", false, 987654321);
        
        args.AccessoryId.Should().Be("mittens");
        args.Equipped.Should().BeFalse();
        args.PlayerId.Should().Be(987654321);
    }

    [Fact]
    public void CareActionCompletedEventArgs_HasCorrectProperties()
    {
        var args = new CareActionCompletedEventArgs(
            "diaper_change", 123456789, 42, "Farmhouse", "context");
        
        args.CareActionId.Should().Be("diaper_change");
        args.PlayerId.Should().Be(123456789);
        args.AbsoluteDay.Should().Be(42);
        args.LocationName.Should().Be("Farmhouse");
        args.Context.Should().Be("context");
    }

    [Fact]
    public void CareActionCompletedEventArgs_LocationAndContextAreOptional()
    {
        var args = new CareActionCompletedEventArgs(
            "feeding", 123456789, 42);
        
        args.CareActionId.Should().Be("feeding");
        args.LocationName.Should().BeNull();
        args.Context.Should().BeNull();
    }
}