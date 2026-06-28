using AgeRegression.Config;
using AgeRegression.Data;
using AgeRegression.Events;
using AgeRegression.State;
using AgeRegression.Utilities;
using DataLoader = AgeRegression.Data.DataLoader;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgeRegression.Systems.Needs;

/// <summary>
/// Manages the continence hidden stat. Drain rate is influenced by
/// regression stage depth, comfort/stress, and equipped diaper type.
///
/// <para>
/// Fires <see cref="ContinenceThresholdCrossedEventArgs"/> and
/// <see cref="NeedThresholdCrossedEventArgs"/> via the event bus.
/// <see cref="Systems.DiaperSystem"/> can subscribe to react to
/// loss-of-control events without any direct coupling.
/// </para>
/// </summary>
public sealed class ContinenceSystem
{
    private readonly StateManager _stateManager;
    private readonly DataLoader _dataLoader;
    private readonly ModConfig _config;
    private readonly ModEventBus _eventBus;
    private readonly LogHelper _log;
    private readonly ContinenceDrainStrategy _drainStrategy;

    private int _lastTick = 0;

    private List<NeedsThresholdData>? _thresholds;

    public ContinenceSystem(
        StateManager stateManager,
        DataLoader dataLoader,
        ModConfig config,
        ModEventBus eventBus,
        LogHelper log)
    {
        _stateManager  = stateManager;
        _dataLoader    = dataLoader;
        _config        = config;
        _eventBus      = eventBus;
        _log           = log;
        _drainStrategy = new ContinenceDrainStrategy(
            config.Needs.Continence, dataLoader);
    }

    public void OnDayStarted()
    {
        _lastTick = Game1.timeOfDay;

        var state = _stateManager.GetCurrentState();
        if (state is not null)
            state.Needs.Continence.LostControlToday = false;
    }

    public void OnTimeChanged(int newTime)
    {
        if (!_config.Needs.Continence.Enabled) return;

        var elapsed = GameTimeHelper.MinutesBetween(_lastTick, newTime);
        if (elapsed < _config.Needs.Continence.TickIntervalMinutes) return;

        _lastTick = newTime;

        var state = _stateManager.GetCurrentState();
        if (state is null) return;

        ProcessTick(state, elapsed);
    }

    public void ResetTick()
    {
        _lastTick = GameHelper.IsWorldReady()
            ? Game1.timeOfDay
            : 0;
    }

    public void SetStressModifier(float stressModifier)
    {
        var state = _stateManager.GetCurrentState();
        if (state is null) return;

        state.Needs.Continence.StressModifier =
            Math.Clamp(stressModifier, -1f, 1f);
    }

    public void Restore(float normalizedAmount)
    {
        var state = _stateManager.GetCurrentState();
        if (state is null) return;

        var before = state.Needs.Continence.Value.Normalized;
        state.Needs.Continence.Value.ApplyDelta(-Math.Abs(normalizedAmount));

        PublishValueChanged(state, "continence", before,
            state.Needs.Continence.Value.Normalized);
        CheckThresholds(state, "continence",
            state.Needs.Continence.Value, before);
    }

    private void ProcessTick(PlayerRegressionState state, int elapsedMinutes)
    {
        var before = state.Needs.Continence.Value.Normalized;
        var drain  = _drainStrategy.ComputeDrain(state, elapsedMinutes);

        if (Math.Abs(drain) < 0.0001f) return;

        state.Needs.Continence.Value.ApplyDelta(drain);
        var after = state.Needs.Continence.Value.Normalized;

        _log.Trace("Continence tick: {0:P1} → {1:P1} (drain={2:F4}).",
            before, after, drain);

        PublishValueChanged(state, "continence", before, after);
        CheckThresholds(state, "continence",
            state.Needs.Continence.Value, before);
    }

    private void CheckThresholds(
        PlayerRegressionState state,
        string needId,
        NeedsValue value,
        float previousNormalized)
    {
        var thresholds = GetThresholds();
        if (thresholds.Count == 0) return;

        var previousBand = FindThreshold(thresholds, previousNormalized);
        var currentBand  = FindThreshold(thresholds, value.Normalized);

        if (previousBand?.Id == currentBand?.Id) return;

        var previousId = previousBand?.Id ?? string.Empty;
        var currentId  = currentBand?.Id  ?? string.Empty;

        value.LastKnownThresholdId = currentId;

        var isDeteriorating = value.Normalized < previousNormalized;
        var isLowestBand    = currentBand is not null &&
            thresholds.All(t => t.MinNormalized >= currentBand.MinNormalized);

        _log.Debug(
            "Continence threshold crossed: '{0}' → '{1}' ({2:P1}).",
            previousId, currentId, value.Normalized);

        _eventBus.Publish(new NeedThresholdCrossedEventArgs(
            needId,
            previousId,
            currentId,
            value.Normalized,
            isDeteriorating,
            state.PlayerId));

        _eventBus.Publish(new ContinenceThresholdCrossedEventArgs(
            previousId,
            currentId,
            value.Normalized,
            isLowestBand && isDeteriorating,
            state.Diaper.IsWearingDiaper,
            state.PlayerId));

        if (isLowestBand && isDeteriorating &&
            !state.Needs.Continence.LostControlToday)
        {
            state.Needs.Continence.LostControlToday = true;
            _log.Debug("Continence: loss of control event fired.");
        }
    }

    private List<NeedsThresholdData> GetThresholds()
    {
        if (_thresholds is not null) return _thresholds;

        var sets = _dataLoader.NeedsThresholdSets;
        var set  = sets.FirstOrDefault(s =>
            string.Equals(s.NeedId, "continence",
                StringComparison.OrdinalIgnoreCase));

        _thresholds = set?.Thresholds
            .OrderBy(t => t.MinNormalized)
            .ToList()
            ?? new List<NeedsThresholdData>();

        return _thresholds;
    }

    private static NeedsThresholdData? FindThreshold(
        List<NeedsThresholdData> thresholds,
        float normalized)
    {
        return thresholds
            .Where(t => t.MinNormalized <= normalized)
            .OrderByDescending(t => t.MinNormalized)
            .FirstOrDefault();
    }

    private void PublishValueChanged(
        PlayerRegressionState state,
        string needId,
        float before,
        float after)
    {
        if (Math.Abs(before - after) < 0.0001f) return;

        _eventBus.Publish(new NeedsValueChangedEventArgs(
            needId, before, after, state.PlayerId));
    }
}
