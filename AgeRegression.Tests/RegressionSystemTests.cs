using AgeRegression.Data;
using AgeRegression.Systems;
using FluentAssertions;

namespace AgeRegression.Tests;

public sealed class RegressionSystemTests
{
    [Fact]
    public void Identity_HasAllMultipliersAtOne()
    {
        var id = EffectiveStatModifiers.Identity;
        id.SpeedMultiplier.Should().Be(1f);
        id.MaxEnergyMultiplier.Should().Be(1f);
        id.SkillXpMultiplier.Should().Be(1f);
        id.CanUseTools.Should().BeTrue();
    }

    [Fact]
    public void DefaultStage_None_HasIdentityModifiers()
    {
        var none = DataLoader.GetDefaultStages().First(s => s.Id == "none");
        none.StatModifiers.SpeedMultiplier.Should().Be(1.0f);
        none.StatModifiers.CanUseTools.Should().BeTrue();
    }

    [Fact]
    public void DefaultStage_Baby_HasReducedModifiers()
    {
        var baby = DataLoader.GetDefaultStages().First(s => s.Id == "baby");
        baby.StatModifiers.SpeedMultiplier.Should().BeLessThan(1.0f);
        baby.StatModifiers.CanUseTools.Should().BeFalse();
    }

    [Fact]
    public void DefaultStages_ModifiersDecreaseWithOrder()
    {
        var stages = DataLoader.GetDefaultStages()
            .OrderBy(s => s.Order).ToList();
        for (var i = 1; i < stages.Count; i++)
        {
            stages[i].StatModifiers.SpeedMultiplier
                .Should().BeLessOrEqualTo(
                    stages[i - 1].StatModifiers.SpeedMultiplier);
        }
    }
}
