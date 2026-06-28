using DataLoader = AgeRegression.Data.DataLoader;
using AgeRegression.Config;
using AgeRegression.Events;
using AgeRegression.State;
using AgeRegression.Utilities;
using StardewValley;
using System;
using System.Linq;

namespace AgeRegression.Systems;

/// <summary>
/// Publishes in-game HUD notifications for significant state changes.
/// Subscribes to the internal event bus and uses Stardew's
/// <see cref="Game1.addHUDMessage"/> to display toast messages.
/// </summary>
public sealed class NotificationSystem
{
    private readonly StateManager _stateManager;
    private readonly DataLoader _dataLoader;
    private readonly ModConfig _config;
    private readonly NotificationConfig _notificationConfig;
    private readonly ModEventBus _eventBus;
    private readonly LogHelper _log;

    private string _lastNotifiedStageId = string.Empty;
    private float _lastNotifiedComfort = -1f;
    private string _lastNotifiedMoodId = string.Empty;

    public NotificationSystem(
        StateManager stateManager,
        DataLoader dataLoader,
        ModConfig config,
        ModEventBus eventBus,
        LogHelper log)
    {
        _stateManager = stateManager;
        _dataLoader   = dataLoader;
        _config       = config;
        _notificationConfig = config.Notifications;
        _eventBus     = eventBus;
        _log          = log;

        eventBus.Subscribe<RegressionChangedEventArgs>(OnRegressionChanged);
        eventBus.Subscribe<AccidentOccurredEventArgs>(OnAccidentOccurred);
        eventBus.Subscribe<ComfortChangedEventArgs>(OnComfortChanged);
        eventBus.Subscribe<MoodChangedEventArgs>(OnMoodChanged);
    }

    /// <summary>
    /// Clears notification history. Call on save load to avoid
    /// replaying notifications for state that existed before the mod was loaded.
    /// </summary>
    public void OnSaveLoaded()
    {
        _lastNotifiedStageId = string.Empty;
        _lastNotifiedComfort = -1f;
        _lastNotifiedMoodId = string.Empty;

        var state = _stateManager.GetCurrentState();
        if (state is not null)
        {
            _lastNotifiedStageId = state.CurrentStageId;
            _lastNotifiedComfort = state.Comfort.CurrentComfort;
            _lastNotifiedMoodId = state.Mood.CurrentMoodId;
        }
    }

    private void OnRegressionChanged(RegressionChangedEventArgs e)
    {
        if (!_config.Enabled || !_notificationConfig.Enabled)
            return;

        if (!GameHelper.IsWorldReady())
            return;

        // Avoid duplicate notifications if the stage hasn't meaningfully changed
        if (_lastNotifiedStageId == e.NewStage.Id)
            return;

        _lastNotifiedStageId = e.NewStage.Id;

        var message = e.NewStage.Order > e.PreviousStage.Order
            ? $"Regression deepened: {e.NewStage.DisplayName}"
            : $"Regression lessened: {e.NewStage.DisplayName}";

        ShowNotification(message);
    }

    private void OnAccidentOccurred(AccidentOccurredEventArgs e)
    {
        if (!_config.Enabled || !_notificationConfig.Enabled)
            return;

        if (!GameHelper.IsWorldReady())
            return;

        var message = e.AccidentType == AccidentType.Wetting
            ? "Wetting accident occurred!"
            : "Messing accident occurred!";

        ShowNotification(message, isUrgent: true);
    }

    private void OnComfortChanged(ComfortChangedEventArgs e)
    {
        if (!_config.Enabled || !_notificationConfig.Enabled)
            return;

        if (!GameHelper.IsWorldReady())
            return;

        var delta = Math.Abs(e.NewComfort - e.PreviousComfort);
        if (delta < _notificationConfig.MinComfortChangeForNotification)
            return;

        // Avoid notifying on every tiny fluctuation
        if (Math.Abs(e.NewComfort - _lastNotifiedComfort) < _notificationConfig.MinComfortChangeForNotification)
            return;

        _lastNotifiedComfort = e.NewComfort;

        var direction = e.NewComfort > e.PreviousComfort ? "increased" : "decreased";
        var message = $"Comfort {direction} ({e.PreviousComfort:F0} → {e.NewComfort:F0})";

        ShowNotification(message);
    }

    private void OnMoodChanged(MoodChangedEventArgs e)
    {
        if (!_config.Enabled || !_notificationConfig.Enabled)
            return;

        if (!GameHelper.IsWorldReady())
            return;

        var previousMood = _dataLoader.GetMoodLevel(e.PreviousMoodId);
        var newMood = _dataLoader.GetMoodLevel(e.NewMoodId);

        if (previousMood is null || newMood is null)
            return;

        // Calculate tier distance
        var tierDelta = Math.Abs(newMood.Order - previousMood.Order);
        if (tierDelta < _notificationConfig.MinMoodTierChangeForNotification)
            return;

        _lastNotifiedMoodId = e.NewMoodId;

        var direction = newMood.Order > previousMood.Order ? "worsened" : "improved";
        var message = $"Mood {direction}: {newMood.DisplayName}";

        ShowNotification(message);
    }

    private void ShowNotification(string message, bool isUrgent = false)
    {
        try
        {
            var hudMessage = new HUDMessage(message)
            {
                timeLeft = isUrgent ? 3500f : 2500f
            };

            Game1.addHUDMessage(hudMessage);
            _log.Debug("Notification shown: {0}", message);
        }
        catch (Exception ex)
        {
            _log.Exception("Failed to show HUD notification", ex);
        }
    }
}