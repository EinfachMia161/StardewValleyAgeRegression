using AgeRegression.Config;
using AgeRegression.Data;
using AgeRegression.Events;
using AgeRegression.Systems;
using AgeRegression.Utilities;
using StardewModdingAPI;
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
    public void OnSaveLoaded_WhenDisabled_DoesNotThrow()
    {
        // Arrange
        var eventBus = new ModEventBus(new LogHelper(new TestMonitor()));
        var config = CreateTestConfig(c => c.Enabled = false);
        var system = CreateNotificationSystem(eventBus, config);

        // Act - should not throw
        system.OnSaveLoaded();

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void OnRegressionChanged_WhenDisabled_DoesNotThrow()
    {
        // Arrange
        var eventBus = new ModEventBus(new LogHelper(new TestMonitor()));
        var config = CreateTestConfig(c => c.Enabled = false);
        var system = CreateNotificationSystem(eventBus, config);

        var args = new RegressionChangedEventArgs(
            new RegressionStageData { Id = "baseline", Order = 0, DisplayName = "Baseline" },
            new RegressionStageData { Id = "regressed", Order = 1, DisplayName = "Regressed" },
            123);

        // Act - should not throw
        system.OnSaveLoaded();
        PublishEvent(eventBus, args);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void OnAccidentOccurred_WhenDisabled_DoesNotThrow()
    {
        // Arrange
        var eventBus = new ModEventBus(new LogHelper(new TestMonitor()));
        var config = CreateTestConfig(c => c.Enabled = false);
        var system = CreateNotificationSystem(eventBus, config);

        var args = new AccidentOccurredEventArgs(AccidentType.Wetting, 123, true);

        // Act
        system.OnSaveLoaded();
        PublishEvent(eventBus, args);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void OnMoodChanged_WhenDisabled_DoesNotThrow()
    {
        // Arrange
        var eventBus = new ModEventBus(new LogHelper(new TestMonitor()));
        var config = CreateTestConfig(c => c.Enabled = false);
        var system = CreateNotificationSystem(eventBus, config);

        var args = new MoodChangedEventArgs("happy", "sad", 123);

        // Act
        system.OnSaveLoaded();
        PublishEvent(eventBus, args);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void OnComfortChanged_WhenDisabled_DoesNotThrow()
    {
        // Arrange
        var eventBus = new ModEventBus(new LogHelper(new TestMonitor()));
        var config = CreateTestConfig(c => c.Enabled = false);
        var system = CreateNotificationSystem(eventBus, config);

        var args = new ComfortChangedEventArgs(50f, 60f, 123, "test");

        // Act
        system.OnSaveLoaded();
        PublishEvent(eventBus, args);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void OnRegressionChanged_WhenNotificationsDisabled_DoesNotThrow()
    {
        // Arrange
        var eventBus = new ModEventBus(new LogHelper(new TestMonitor()));
        var config = CreateTestConfig();
        config.Notifications.Enabled = false;
        var system = CreateNotificationSystem(eventBus, config);

        var args = new RegressionChangedEventArgs(
            new RegressionStageData { Id = "baseline", Order = 0, DisplayName = "Baseline" },
            new RegressionStageData { Id = "regressed", Order = 1, DisplayName = "Regressed" },
            123);

        // Act
        system.OnSaveLoaded();
        PublishEvent(eventBus, args);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void OnAccidentOccurred_WhenNotificationsDisabled_DoesNotThrow()
    {
        // Arrange
        var eventBus = new ModEventBus(new LogHelper(new TestMonitor()));
        var config = CreateTestConfig();
        config.Notifications.Enabled = false;
        var system = CreateNotificationSystem(eventBus, config);

        var args = new AccidentOccurredEventArgs(AccidentType.Messing, 123, true);

        // Act
        system.OnSaveLoaded();
        PublishEvent(eventBus, args);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void OnMoodChanged_WhenNotificationsDisabled_DoesNotThrow()
    {
        // Arrange
        var eventBus = new ModEventBus(new LogHelper(new TestMonitor()));
        var config = CreateTestConfig();
        config.Notifications.Enabled = false;
        var system = CreateNotificationSystem(eventBus, config);

        var args = new MoodChangedEventArgs("happy", "sad", 123);

        // Act
        system.OnSaveLoaded();
        PublishEvent(eventBus, args);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void OnComfortChanged_WhenBelowThreshold_DoesNotThrow()
    {
        // Arrange
        var eventBus = new ModEventBus(new LogHelper(new TestMonitor()));
        var config = CreateTestConfig(c => c.MinComfortChangeForNotification = 100f);
        var system = CreateNotificationSystem(eventBus, config);

        var args = new ComfortChangedEventArgs(50f, 60f, 123, "test");

        // Act
        system.OnSaveLoaded();
        PublishEvent(eventBus, args);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void OnMoodChanged_WhenBelowTierThreshold_DoesNotThrow()
    {
        // Arrange
        var eventBus = new ModEventBus(new LogHelper(new TestMonitor()));
        var config = CreateTestConfig(c => c.MinMoodTierChangeForNotification = 5);
        var system = CreateNotificationSystem(eventBus, config);

        var args = new MoodChangedEventArgs("happy", "sad", 123);

        // Act
        system.OnSaveLoaded();
        PublishEvent(eventBus, args);

        // Assert
        Assert.True(true);
    }

    private static NotificationSystem CreateNotificationSystem(
        ModEventBus eventBus,
        ModConfig? config = null)
    {
        config ??= CreateTestConfig();

        return new NotificationSystem(
            stateManager: null!,
            dataLoader: null!,
            config,
            eventBus,
            log: new LogHelper(new TestMonitor()));
    }

    private static void PublishEvent<TEvent>(ModEventBus eventBus, TEvent args)
        where TEvent : class
    {
        typeof(ModEventBus)
            .GetMethod("Publish", new[] { typeof(TEvent) })!
            .Invoke(eventBus, new object[] { args });
    }
}

/// <summary>
/// Minimal monitor implementation for testing.
/// </summary>
internal sealed class TestMonitor : IMonitor
{
    public void Log(string message) { }
    public void Log(string message, LogLevel level) { }
    public void LogOnce(string message) { }
    public void LogOnce(string message, LogLevel level) { }
    public void VerboseLog(string message) { }
    public bool IsVerbose { get; set; }
}
