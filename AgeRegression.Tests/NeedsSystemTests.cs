using AgeRegression.Config;
using AgeRegression.Data;
using AgeRegression.State;
using AgeRegression.Systems.Needs;
using AgeRegression.Utilities;
using FluentAssertions;

namespace AgeRegression.Tests;

public sealed class NeedsSystemTests
{
    [Fact]
    public void NeedsValue_DefaultsToFull()
    {
        new NeedsValue().Normalized.Should().Be(1.0f);
    }

    [Fact]
    public void NeedsValue_ApplyDelta_DrainReducesValue()
    {
        var v = new NeedsValue { Normalized = 1.0f };
        v.ApplyDelta(0.1f);
        v.Normalized.Should().BeApproximately(0.9f, 0.0001f);
    }

    [Fact]
    public void NeedsValue_ApplyDelta_ClampsAtZero()
    {
        var v = new NeedsValue { Normalized = 0.05f };
        v.ApplyDelta(0.5f);
        v.Normalized.Should().Be(0.0f);
    }

    [Fact]
    public void NeedsValue_Clone_IsDeepCopy()
    {
        var original = new NeedsValue
        {
            Normalized = 0.6f, LastKnownThresholdId = "warning"
        };
        var clone = original.Clone();
        clone.Normalized = 0.3f;
        original.Normalized.Should().Be(0.6f);
    }

    [Fact]
    public void TimedDrain_OneHour_ReturnsConfiguredRate()
    {
        var strategy = new TimedDrainStrategy(0.04f);
        var state    = MakeState("none");
        strategy.ComputeDrain(state, 60).Should().BeApproximately(0.04f, 0.0001f);
    }

    [Fact]
    public void TimedDrain_ZeroElapsed_ReturnsZero()
    {
        var strategy = new TimedDrainStrategy(0.04f);
        strategy.ComputeDrain(MakeState("none"), 0).Should().Be(0f);
    }

    [Fact]
    public void ContinenceDrain_AtBaseline_ReturnsNegativeDrain()
    {
        var (strategy, _) = MakeContinenceStrategy();
        strategy.ComputeDrain(MakeState("none"), 60).Should().BeLessThan(0f);
    }

    [Fact]
    public void ContinenceDrain_DeepStage_DrainsFasterThanShallowStage()
    {
        var (strategy, _) = MakeContinenceStrategy();
        var little = strategy.ComputeDrain(MakeState("little"), 60);
        var baby   = strategy.ComputeDrain(MakeState("baby"), 60);
        baby.Should().BeGreaterThan(little);
    }

    [Fact]
    public void Migration_V1ToV2_SetsSchemaVersion2()
    {
        var migrator = new AgeRegression.Persistence.SaveDataMigrator(
            new LogHelper(NullMonitor.Instance));
        var data = new AgeRegression.Persistence.SaveDataModel
        {
            SchemaVersion = 1
        };
        migrator.Migrate(data).Should().BeTrue();
        data.SchemaVersion.Should().Be(2);
    }

    private static PlayerRegressionState MakeState(
        string stageId,
        float stressModifier = 0f)
    {
        return new PlayerRegressionState
        {
            CurrentStageId = stageId,
            Needs = new PlayerNeedsState
            {
                Continence = new ContinenceState
                {
                    Value          = new NeedsValue { Normalized = 1.0f },
                    StressModifier = stressModifier
                }
            }
        };
    }

    private static (ContinenceDrainStrategy strategy, DataLoader loader)
        MakeContinenceStrategy()
    {
        var loader = new DataLoader(new EmptyAssetProvider(), new LogHelper(NullMonitor.Instance));
        loader.LoadAll();
        var config = new ContinenceConfig
        {
            BaseDrainPerHour             = 0.04f,
            MaxRegressionDrainMultiplier = 3.0f,
            StressInfluence              = 0.5f,
            RecoveryPerHour              = 0.1f
        };
        return (new ContinenceDrainStrategy(config, loader), loader);
    }
}
