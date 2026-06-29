using AgeRegression.Data;
using AgeRegression.Events;
using AgeRegression.Persistence;
using AgeRegression.Utilities;
using StardewValley;
using DataLoader = AgeRegression.Data.DataLoader;

namespace AgeRegression.State;

/// <summary>
/// Owns the player's <see cref="PlayerRegressionState"/>, validates all
/// state transitions, and publishes events via the
/// <see cref="ModEventBus"/>.
///
/// <para>
/// This is the single source of truth for player state. All other
/// systems read state from here and request changes through the methods
/// on this class. They never modify
/// <see cref="PlayerRegressionState"/> directly.
/// </para>
/// </summary>
public sealed class StateManager
{
    private readonly DataLoader _dataLoader;
    private readonly PersistenceManager _persistence;
    private readonly ModEventBus _eventBus;
    private readonly LogHelper _log;

    private PlayerRegressionState? _state;

    public StateManager(
        DataLoader dataLoader,
        PersistenceManager persistence,
        ModEventBus eventBus,
        LogHelper log)
    {
        _dataLoader  = dataLoader;
        _persistence = persistence;
        _eventBus    = eventBus;
        _log         = log;
    }

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Loads state for the current local player from their save data.
    /// Must be called after <c>SaveLoaded</c>.
    /// </summary>
    public void LoadForCurrentPlayer()
    {
        var farmer = GameHelper.GetLocalPlayer();
        if (farmer is null)
        {
            _log.Warn("LoadForCurrentPlayer called but no local player found.");
            return;
        }

        var saveData = _persistence.Load(farmer);
        _state = PersistenceManager.StateFromSaveData(
            saveData, farmer.UniqueMultiplayerID);

        // Validate that the loaded stage ID still exists in data
        if (_dataLoader.GetStage(_state.CurrentStageId) is null)
        {
            _log.Warn(
                "Loaded stage ID '{0}' no longer exists in data. " +
                "Resetting to baseline.",
                _state.CurrentStageId);
            _state.CurrentStageId =
                _dataLoader.GetBaselineStage()?.Id ?? "none";
        }

        _log.Info(
            "State loaded for '{0}': stage='{1}', diaper={2}.",
            farmer.Name,
            _state.CurrentStageId,
            _state.Diaper.IsWearingDiaper
                ? _state.Diaper.EquippedDiaperTypeId
                : "none");
    }

    /// <summary>
    /// Saves state for the current local player to their save data.
    /// Must be called during <c>Saving</c>.
    /// </summary>
    public void SaveForCurrentPlayer()
    {
        if (_state is null) return;

        var farmer = GameHelper.GetLocalPlayer();
        if (farmer is null)
        {
            _log.Warn("SaveForCurrentPlayer called but no local player found.");
            return;
        }

        _persistence.SaveFromState(farmer, _state);
    }

    /// <summary>
    /// Replaces the current in-memory state with the given state.
    /// Used by console commands to reset state without touching the
    /// file system directly.
    /// </summary>
    public void ForceLoadState(PlayerRegressionState state)
    {
        _state = state;
        _log.Debug("State force-loaded (e.g. from console reset).");
    }

    /// <summary>
    /// Clears in-memory state when returning to the title screen.
    /// </summary>
    public void Unload()
    {
        _state = null;
        _log.Debug("State unloaded.");
    }

    // -------------------------------------------------------------------------
    // Day / time hooks
    // -------------------------------------------------------------------------

    /// <summary>Called at the start of each in-game day.</summary>
    public void OnDayStarted()
    {
        if (_state is null) return;

        _state.AccidentsToday = 0;
        _state.Needs.Continence.LostControlToday = false;
        _state.LastUpdatedAbsoluteDay = AbsoluteDayHelper.GetCurrentAbsoluteDay();

        PruneDialogueCooldowns();
    }

    /// <summary>Called each time the in-game clock advances.</summary>
    public void OnTimeChanged(int newTime)
    {
        // Reserved for future time-based state updates.
        // Systems subscribe to the event bus rather than being called here.
    }

    // -------------------------------------------------------------------------
    // Accessors (read-only for external consumers)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the current player state, or <c>null</c> if no save is
    /// loaded.
    /// </summary>
    public PlayerRegressionState? GetCurrentState() => _state;

    /// <summary>
    /// Returns the current regression stage data, or <c>null</c> if no
    /// save is loaded.
    /// </summary>
    public RegressionStageData? GetCurrentStage()
    {
        if (_state is null) return null;
        return _dataLoader.GetStage(_state.CurrentStageId);
    }

    /// <summary>
    /// Returns <c>true</c> if the player is currently at any regressed
    /// stage (i.e. not at the baseline stage).
    /// </summary>
    public bool IsRegressed()
    {
        if (_state is null) return false;
        var baseline = _dataLoader.GetBaselineStage();
        return baseline is null || _state.CurrentStageId != baseline.Id;
    }

    // -------------------------------------------------------------------------
    // Mutations
    // -------------------------------------------------------------------------

    /// <summary>
    /// Attempts to transition the player to the specified regression
    /// stage. Publishes <see cref="RegressionChangedEventArgs"/> on
    /// success.
    /// </summary>
    /// <returns><c>true</c> if the transition succeeded.</returns>
    public bool TrySetStage(string stageId)
    {
        if (_state is null)
        {
            _log.Warn("TrySetStage called but state is not loaded.");
            return false;
        }

        var newStage = _dataLoader.GetStage(stageId);
        if (newStage is null)
        {
            _log.Warn("TrySetStage: stage ID '{0}' not found.", stageId);
            return false;
        }

        if (_state.CurrentStageId == stageId)
            return true; // Already at this stage � not an error

        var previousStage = _dataLoader.GetStage(_state.CurrentStageId)
            ?? _dataLoader.GetBaselineStage()!;

        _state.CurrentStageId = stageId;

        _log.Info("Regression stage: '{0}' ? '{1}'.",
            previousStage.Id, newStage.Id);

        _eventBus.Publish(new RegressionChangedEventArgs(
            previousStage, newStage, _state.PlayerId));

        return true;
    }

    /// <summary>
    /// Updates the player's diaper state using value equality to avoid
    /// spurious events when the state has not meaningfully changed.
    /// Publishes <see cref="DiaperStateChangedEventArgs"/> if changed.
    /// </summary>
    public void UpdateDiaperState(DiaperState newState)
    {
        if (_state is null) return;

        var previous = _state.Diaper;

        // Value equality � no event if nothing actually changed
        if (previous == newState) return;

        _state.Diaper = newState;

        _eventBus.Publish(new DiaperStateChangedEventArgs(
            previous, newState, _state.PlayerId));
    }

    /// <summary>
    /// Updates the player's comfort score and publishes
    /// <see cref="ComfortChangedEventArgs"/> if the value changed.
    /// Uses an epsilon comparison to avoid floating-point noise events.
    /// </summary>
    public void UpdateComfort(float newComfort, string reason)
    {
        if (_state is null) return;

        var previous = _state.Comfort.CurrentComfort;
        if (Math.Abs(previous - newComfort) < 0.001f) return;

        _state.Comfort.CurrentComfort = newComfort;

        _eventBus.Publish(new ComfortChangedEventArgs(
            previous, newComfort, _state.PlayerId, reason));
    }

    /// <summary>
    /// Equips an accessory by ID and publishes
    /// <see cref="AccessoryChangedEventArgs"/>.
    /// Returns <c>false</c> if state is not loaded or the ID is already equipped.
    /// </summary>
    public bool EquipAccessory(string accessoryId)
    {
        if (_state is null) return false;

        if (_state.EquippedAccessories.Add(accessoryId))
        {
            _eventBus.Publish(new AccessoryChangedEventArgs(
                accessoryId, true, _state.PlayerId));
            return true;
        }
        return false;
    }

    /// <summary>
    /// Unequips an accessory by ID and publishes
    /// <see cref="AccessoryChangedEventArgs"/>.
    /// Returns <c>false</c> if state is not loaded or the ID was not equipped.
    /// </summary>
    public bool UnequipAccessory(string accessoryId)
    {
        if (_state is null) return false;

        if (_state.EquippedAccessories.Remove(accessoryId))
        {
            _eventBus.Publish(new AccessoryChangedEventArgs(
                accessoryId, false, _state.PlayerId));
            return true;
        }
        return false;
    }

    /// <summary>
    /// Updates the player's continence value and publishes
    /// <see cref="NeedThresholdCrossedEventArgs"/> and
    /// <see cref="ContinenceThresholdCrossedEventArgs"/> if thresholds changed.
    /// Uses an epsilon comparison to avoid spurious events.
    /// </summary>
    public void UpdateContinence(float delta)
    {
        if (_state is null) return;

        var before = _state.Needs.Continence.Value.Normalized;
        _state.Needs.Continence.Value.ApplyDelta(delta);
        var after = _state.Needs.Continence.Value.Normalized;

        // Publish value changed event
        if (Math.Abs(before - after) >= 0.0001f)
        {
            _eventBus.Publish(new NeedsValueChangedEventArgs(
                "continence", before, after, _state.PlayerId));

            // Check thresholds
            var thresholds = _dataLoader.NeedsThresholdSets
                .Where(s => s.NeedId.Equals("continence", StringComparison.OrdinalIgnoreCase))
                .SelectMany(s => s.Thresholds)
                .OrderBy(t => t.MinNormalized)
                .ToList();

            var previousBand = thresholds.LastOrDefault(t => t.MinNormalized <= before);
            var currentBand = thresholds.LastOrDefault(t => t.MinNormalized <= after);

            if (previousBand?.Id != currentBand?.Id)
            {
                var previousId = previousBand?.Id ?? string.Empty;
                var currentId = currentBand?.Id ?? string.Empty;

                _state.Needs.Continence.Value.LastKnownThresholdId = currentId;

                var isDeteriorating = after < before;
                var isLowestBand = currentBand != null &&
                    thresholds.All(t => t.MinNormalized >= currentBand.MinNormalized);

                _log.Debug(
                    "Continence threshold crossed: '{0}' → '{1}' ({2:P1}).",
                    previousId, currentId, after);

                _eventBus.Publish(new NeedThresholdCrossedEventArgs(
                    "continence",
                    previousId,
                    currentId,
                    after,
                    isDeteriorating,
                    _state.PlayerId));

                _eventBus.Publish(new ContinenceThresholdCrossedEventArgs(
                    previousId,
                    currentId,
                    after,
                    isLowestBand && isDeteriorating,
                    _state.Diaper.IsWearingDiaper,
                    _state.PlayerId));
            }
        }
    }

    /// <summary>
    /// Updates the player's mood ID and publishes
    /// <see cref="MoodChangedEventArgs"/> if it changed.
    /// </summary>
    public void UpdateMood(string newMoodId)
    {
        if (_state is null) return;

        var previousId = _state.Mood.CurrentMoodId;
        if (previousId == newMoodId) return;

        _state.Mood.CurrentMoodId = newMoodId;

        _eventBus.Publish(new MoodChangedEventArgs(
            previousId, newMoodId, _state.PlayerId));
    }

    /// <summary>
    /// Records an accident and publishes <see cref="AccidentOccurredEventArgs"/>.
    /// </summary>
    public void RecordAccident(AccidentType type)
    {
        if (_state is null) return;

        _state.AccidentsToday++;

        _eventBus.Publish(new AccidentOccurredEventArgs(
            type, _state.PlayerId, _state.Diaper.IsWearingDiaper));
    }

    /// <summary>
    /// Records that a dialogue entry was shown today for cooldown
    /// tracking. Uses absolute day numbers for cross-season stability.
    /// </summary>
    public void RecordDialogueShown(string npcName, string dialogueKey)
    {
        if (_state is null) return;
        _state.DialogueCooldowns[$"{npcName}:{dialogueKey}"] =
            AbsoluteDayHelper.GetCurrentAbsoluteDay();
    }

    /// <summary>
    /// Returns <c>true</c> if the given dialogue entry is on cooldown.
    /// </summary>
    public bool IsDialogueOnCooldown(
        string npcName,
        string dialogueKey,
        int cooldownDays)
    {
        if (_state is null || cooldownDays <= 0) return false;

        var key = $"{npcName}:{dialogueKey}";
        if (!_state.DialogueCooldowns.TryGetValue(key, out var lastShownDay))
            return false;

        var currentDay = AbsoluteDayHelper.GetCurrentAbsoluteDay();
        return AbsoluteDayHelper.DaysBetween(lastShownDay, currentDay)
               < cooldownDays;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private void PruneDialogueCooldowns()
    {
        if (_state is null) return;

        const int maxCooldownAge = 30;
        var currentDay = AbsoluteDayHelper.GetCurrentAbsoluteDay();

        var expired = _state.DialogueCooldowns
            .Where(kvp =>
                AbsoluteDayHelper.DaysBetween(kvp.Value, currentDay)
                >= maxCooldownAge)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expired)
            _state.DialogueCooldowns.Remove(key);

        if (expired.Count > 0)
            _log.Trace("Pruned {0} expired dialogue cooldowns.", expired.Count);
    }
}
