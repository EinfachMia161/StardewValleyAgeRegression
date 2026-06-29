using AgeRegression.Config;
using AgeRegression.Data;
using AgeRegression.Events;
using AgeRegression.Systems;
using AgeRegression.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Framework.Logging;
using Xunit;


namespace AgeRegression.Tests;

public sealed class NotificationSystemTests
{
    private static ModConfig CreateTestConfig(Action<NotificationConfig>? configure = null)
    {
        var config = new ModConfig
        {
            Enabled = true,
            Notifications = new NotificationConfig()
        };

        configure?.Invoke(config.Notifications);
        return config;
    }

    [Fact]
    public void NotificationSystem_WhenDisabled_DoesNotThrow()
    {
        // Arrange
        var config = CreateTestConfig(c => c.Enabled = false);
        var eventBus = new ModEventBus(new LogHelper(new TestMonitor()));

        // Act - should not throw on construction
        var system = new NotificationSystem(
            stateManager: null!,
            dataLoader: null!,
            config,
            eventBus,
            log: new LogHelper(new TestMonitor()));

        // Assert - system was created
        Assert.NotNull(system);
    }

    [Fact]
    public void NotificationSystem_WhenNotificationsDisabled_DoesNotThrow()
    {
        // Arrange
        var config = CreateTestConfig();
        config.Notifications.Enabled = false;
        var eventBus = new ModEventBus(new LogHelper(new TestMonitor()));

        // Act - should not throw on construction
        var system = new NotificationSystem(
            stateManager: null!,
            dataLoader: null!,
            config,
            eventBus,
            log: new LogHelper(new TestMonitor()));

        // Assert - system was created
        Assert.NotNull(system);
    }
}

/// <summary>
/// Minimal monitor implementation for testing.
/// </summary>
internal sealed class TestMonitor : IMonitor
{
    public bool IsVerbose { get; set; }

    public void Log(string message, LogLevel level = LogLevel.Trace)
    {
    }

    public void LogOnce(string message, LogLevel level = LogLevel.Trace)
    {
    }

    public void VerboseLog(string message)
    {
    }

    public void VerboseLog(ref VerboseLogStringHandler message)
    {
    }
}