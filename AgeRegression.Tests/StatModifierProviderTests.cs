using AgeRegression.Data;
using AgeRegression.Systems;
using FluentAssertions;

namespace AgeRegression.Tests;

public sealed class StatModifierProviderTests
{
    [Fact]
    public void Identity_HasAllMultipliersAtOne()
    {
        var id = StatModifierContribution.Identity;
        id.SpeedMultiplier.Should().Be(1f);
        id.MaxEnergyMultiplier.Should().Be(1f);
        id.SkillXpMultiplier.Should().Be(1f);
        id.CanUseTools.Should().BeTrue();
    }

    [Fact]
    public void MultipleProviders_MultipliedTogether()
    {
        var providers = new List<IStatModifierProvider>
        {
            new StubProvider("a",
                new StatModifierContribution(0.9f, 1f, 0.8f, true)),
            new StubProvider("b",
                new StatModifierContribution(0.95f, 1f, 1.1f, true))
        };

        var speed = providers.Aggregate(1f,
            (acc, p) => acc * p.GetContribution().SpeedMultiplier);
        speed.Should().BeApproximately(0.9f * 0.95f, 0.001f);
    }

    [Fact]
    public void CanUseTools_FalseIfAnyProviderReturnsFalse()
    {
        var providers = new List<IStatModifierProvider>
        {
            new StubProvider("a",
                new StatModifierContribution(1f, 1f, 1f, true)),
            new StubProvider("b",
                new StatModifierContribution(1f, 1f, 1f, false))
        };
        providers.All(p => p.GetContribution().CanUseTools).Should().BeFalse();
    }

    [Fact]
    public void HappyMood_HasPositiveContribution()
    {
        var happy = DataLoader.GetDefaultMoodLevels()
            .First(m => m.Id == "happy");
        happy.StatModifiers.SpeedMultiplier.Should().BeGreaterThan(1f);
        happy.StatModifiers.SkillXpMultiplier.Should().BeGreaterThan(1f);
    }

    [Fact]
    public void DistressedMood_HasNegativeContribution()
    {
        var distressed = DataLoader.GetDefaultMoodLevels()
            .First(m => m.Id == "distressed");
        distressed.StatModifiers.SpeedMultiplier.Should().BeLessThan(1f);
        distressed.StatModifiers.SkillXpMultiplier.Should().BeLessThan(1f);
    }

    private sealed class StubProvider : IStatModifierProvider
    {
        private readonly StatModifierContribution _contribution;
        public string ProviderId { get; }
        public StubProvider(string id, StatModifierContribution c)
        {
            ProviderId    = id;
            _contribution = c;
        }
        public StatModifierContribution GetContribution() => _contribution;
    }
}
