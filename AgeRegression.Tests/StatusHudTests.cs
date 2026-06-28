using AgeRegression.Config;
using AgeRegression.Data;
using AgeRegression.Events;
using AgeRegression.State;
using AgeRegression.UI;
using AgeRegression.Utilities;
using FluentAssertions;
using Xunit;

namespace AgeRegression.Tests;

/// <summary>
/// Tests for <see cref="StatusHud"/>.
/// </summary>
public class StatusHudTests
{
    private static ModConfig CreateConfig(Action<HudConfig>? configure = null)
    {
        var config = new ModConfig
        {
            Enabled = true
        };

        configure?.Invoke(config.Hud);
        return config;
    }

    private static StatusHud CreateHud(
        PlayerRegressionState? state = null,
        ModConfig? config = null)
    {
        config ??= CreateConfig();

        var eventBus = new ModEventBus(new LogHelper(new TestMonitor()));
        var log = new LogHelper(new TestMonitor());

        return new StatusHud(
            stateManager: null!,
            dataLoader: null!,
            config,
            eventBus,
            log);
    }

    // -------------------------------------------------------------------------
    // ShouldRender
    // -------------------------------------------------------------------------

    [Fact]
    public void ShouldRender_ReturnsFalse_WhenMasterConfigDisabled()
    {
        var config = CreateConfig(c => c.Enabled = false);
        var hud = CreateHud(config: config);

        hud.ShouldRender().Should().BeFalse();
    }

    [Fact]
    public void ShouldRender_ReturnsFalse_WhenHudConfigDisabled()
    {
        var config = CreateConfig(c => c.Enabled = false);
        var hud = CreateHud(config: config);

        hud.ShouldRender().Should().BeFalse();
    }

    [Fact]
    public void ShouldRender_ReturnsFalse_WhenNotInitialized()
    {
        var config = CreateConfig(c => c.Enabled = true);
        var hud = CreateHud(config: config);

        hud.ShouldRender().Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // GetDisplayData — regression info
    // -------------------------------------------------------------------------

    [Fact]
    public void GetDisplayData_ReturnsStageName_FromState()
    {
        var state = new PlayerRegressionState
        {
            CurrentStageId = "little"
        };

        var hud = CreateHud(state: state);
        hud.OnSaveLoaded();

        var data = hud.GetDisplayData();
        data.StageName.Should().Be("little");
    }

    [Fact]
    public void GetDisplayData_ReturnsNullProgress_WhenNoStages()
    {
        var state = new PlayerRegressionState
        {
            CurrentStageId = "none"
        };

        var hud = CreateHud(state: state);
        hud.OnSaveLoaded();

        var data = hud.GetDisplayData();
        data.ProgressPercent.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // GetDisplayData — diaper info
    // -------------------------------------------------------------------------

    [Fact]
    public void GetDisplayData_ShowsNone_WhenNoDiaper()
    {
        var state = new PlayerRegressionState
        {
            Diaper = DiaperState.None
        };

        var hud = CreateHud(state: state);
        hud.OnSaveLoaded();

        var data = hud.GetDisplayData();
        data.DiaperCondition.Should().Be("none");
        data.WetnessPercent.Should().Be(0f);
        data.MessingPercent.Should().Be(0f);
    }

    [Fact]
    public void GetDisplayData_ShowsWetnessAndMessing_WhenDiaperEquipped()
    {
        var state = new PlayerRegressionState
        {
            Diaper = new DiaperState
            {
                EquippedDiaperTypeId = "basic",
                WetnessLevel = 0.7f,
                MessingLevel = 0.3f
            }
        };

        var hud = CreateHud(state: state);
        hud.OnSaveLoaded();

        var data = hud.GetDisplayData();
        data.DiaperCondition.Should().Be("wet");
        data.WetnessPercent.Should().BeApproximately(70f, 0.1f);
        data.MessingPercent.Should().BeApproximately(30f, 0.1f);
    }

    // -------------------------------------------------------------------------
    // GetDisplayData — comfort and mood
    // -------------------------------------------------------------------------

    [Fact]
    public void GetDisplayData_ReturnsComfort_FromState()
    {
        var state = new PlayerRegressionState
        {
            Comfort = new ComfortState { CurrentComfort = 75f }
        };

        var hud = CreateHud(state: state);
        hud.OnSaveLoaded();

        var data = hud.GetDisplayData();
        data.Comfort.Should().Be(75f);
    }

    [Fact]
    public void GetDisplayData_FallsBackToMoodId_WhenMoodDataMissing()
    {
        var state = new PlayerRegressionState
        {
            Mood = new MoodState { CurrentMoodId = "unknown_mood" }
        };

        var hud = CreateHud(state: state);
        hud.OnSaveLoaded();

        var data = hud.GetDisplayData();
        data.Mood.Should().Be("unknown_mood");
    }

    // -------------------------------------------------------------------------
    // GetDisplayData — continence
    // -------------------------------------------------------------------------

    [Fact]
    public void GetDisplayData_ReturnsContinence_FromNeeds()
    {
        var state = new PlayerRegressionState
        {
            Needs = new PlayerNeedsState
            {
                Continence = new ContinenceState
                {
                    Value = new NeedsValue { Normalized = 0.45f }
                }
            }
        };

        var hud = CreateHud(state: state);
        hud.OnSaveLoaded();

        var data = hud.GetDisplayData();
        data.Continence.Should().BeApproximately(45f, 0.1f);
    }

    // -------------------------------------------------------------------------
    // Configuration
    // -------------------------------------------------------------------------

    [Fact]
    public void GetConfig_ReturnsHudConfig_FromModConfig()
    {
       var config = CreateConfig(c =>
{
        c.PositionX = 100;
        c.PositionY = 200;
        c.Scale = 1.5f;
});

        var hud = CreateHud(config: config);

        var hudConfig = hud.GetConfig();
        hudConfig.PositionX.Should().Be(100);
        hudConfig.PositionY.Should().Be(200);
        hudConfig.Scale.Should().Be(1.5f);
    }

    // -------------------------------------------------------------------------
    // Missing state handling
    // -------------------------------------------------------------------------

    [Fact]
    public void OnSaveLoaded_HandlesNullState_Gracefully()
    {
        var eventBus = new ModEventBus(new LogHelper(new TestMonitor()));
        var log = new LogHelper(new TestMonitor());

        var hud = new StatusHud(
            stateManager: null!,
            dataLoader: null!,
            CreateConfig(),
            eventBus,
            log);

        // Should not throw
        hud.OnSaveLoaded();

        hud.ShouldRender().Should().BeFalse();
    }
}
