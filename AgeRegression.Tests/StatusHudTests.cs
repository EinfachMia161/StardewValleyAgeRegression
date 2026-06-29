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

    // -------------------------------------------------------------------------
    // ShouldRender
    // -------------------------------------------------------------------------

    [Fact]
    public void ShouldRender_ReturnsFalse_WhenMasterConfigDisabled()
    {
        var config = CreateConfig(c => c.Enabled = false);
        var eventBus = new ModEventBus(new LogHelper(new TestMonitor()));
        var log = new LogHelper(new TestMonitor());

        var hud = new StatusHud(
            stateManager: null!,
            dataLoader: null!,
            config,
            eventBus,
            log);

        hud.ShouldRender().Should().BeFalse();
    }

    [Fact]
    public void ShouldRender_ReturnsFalse_WhenHudConfigDisabled()
    {
        var config = CreateConfig(c => c.Enabled = false);
        var eventBus = new ModEventBus(new LogHelper(new TestMonitor()));
        var log = new LogHelper(new TestMonitor());

        var hud = new StatusHud(
            stateManager: null!,
            dataLoader: null!,
            config,
            eventBus,
            log);

        hud.ShouldRender().Should().BeFalse();
    }

    [Fact]
    public void ShouldRender_ReturnsFalse_WhenNotInitialized()
    {
        var config = CreateConfig(c => c.Enabled = true);
        var eventBus = new ModEventBus(new LogHelper(new TestMonitor()));
        var log = new LogHelper(new TestMonitor());

        var hud = new StatusHud(
            stateManager: null!,
            dataLoader: null!,
            config,
            eventBus,
            log);

        hud.ShouldRender().Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // GetDisplayData — defaults when uninitialized
    // -------------------------------------------------------------------------

    [Fact]
    public void GetDisplayData_ReturnsDefaults_WhenNotInitialized()
    {
        var eventBus = new ModEventBus(new LogHelper(new TestMonitor()));
        var log = new LogHelper(new TestMonitor());

        var hud = new StatusHud(
            stateManager: null!,
            dataLoader: null!,
            CreateConfig(),
            eventBus,
            log);

        var data = hud.GetDisplayData();
        data.StageName.Should().Be("None");
        data.DiaperCondition.Should().Be("None");
        data.Comfort.Should().Be(0f);
        data.Mood.Should().Be("Neutral");
        data.Continence.Should().Be(0f);
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

        var eventBus = new ModEventBus(new LogHelper(new TestMonitor()));
        var log = new LogHelper(new TestMonitor());

        var hud = new StatusHud(
            stateManager: null!,
            dataLoader: null!,
            config,
            eventBus,
            log);

        var hudConfig = hud.GetConfig();
        hudConfig.PositionX.Should().Be(100);
        hudConfig.PositionY.Should().Be(200);
        hudConfig.Scale.Should().Be(1.5f);
    }
}