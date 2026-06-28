using AgeRegression.Config;
using AgeRegression.Data;
using AgeRegression.Dialogue;
using AgeRegression.Events;
using AgeRegression.State;
using AgeRegression.Utilities;
using DataLoader = AgeRegression.Data.DataLoader;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgeRegression.Systems;

/// <summary>
/// Manages the player's comfort score.
///
/// <para>
/// Comfort is recalculated whenever relevant state changes (diaper,
/// continence, accessories, regression stage, furniture proximity) by
/// subscribing to the internal event bus. A passive tick also runs on
/// <c>TimeChanged</c> to apply per-hour modifiers.
/// </para>
///
/// <para>
/// All modifier values come from
/// <c>assets/data/comfort-modifiers.json</c>. No comfort values are
/// hardcoded in C#.
/// </para>
///
/// <para>
/// <b>ActiveModifiers cache behavior:</b>
/// <see cref="State.ComfortState.ActiveModifiers"/> is a transient
/// cache that records which modifier IDs were active at the last
/// evaluation. It is intentionally NOT persisted to the save file.
/// On save load, <see cref="OnSaveLoaded"/> clears this cache and
/// calls <see cref="RecalculateImmediate"/>. This means every modifier
/// whose conditions are currently satisfied is treated as "newly
/// active" and its <see cref="ComfortModifierData.ImmediateValue"/> is applied. This
/// is correct and intentional — see
/// <see cref="State.ComfortState"/> for full documentation.
/// </para>
/// </summary>
public sealed class ComfortSystem
{
    private readonly StateManager _stateManager;
    private readonly DataLoader _dataLoader;
    private readonly DialogueConditionEvaluator _conditionEvaluator;
    private readonly ModConfig _config;
    private readonly ModEventBus _eventBus;
    private readonly LogHelper _log;

    private int _lastTick = 0;
    private const int TickIntervalMinutes = 20;

    public ComfortSystem(
        StateManager stateManager,
        DataLoader dataLoader,
        DialogueConditionEvaluator conditionEvaluator,
        ModConfig config,
        ModEventBus eventBus,
        LogHelper log)
    {
        _stateManager       = stateManager;
        _dataLoader         = dataLoader;
        _conditionEvaluator = conditionEvaluator;
        _config             = config;
        _eventBus           = eventBus;
        _log                = log;

        eventBus.Subscribe<DiaperStateChangedEventArgs>(OnDiaperStateChanged);
        eventBus.Subscribe<RegressionChangedEventArgs>(OnRegressionChanged);
        eventBus.Subscribe<ContinenceThresholdCrossedEventArgs>(
            OnContinenceThresholdCrossed);
        eventBus.Subscribe<NeedsValueChangedEventArgs>(OnNeedsValueChanged);
        eventBus.Subscribe<FurnitureProximityChangedEventArgs>(
            OnFurnitureProximityChanged);
        eventBus.Subscribe<DiaperComfortEffectEventArgs>(OnDiaperComfortEffect);
    }

    public void OnDayStarted()
    {
        _lastTick = Game1.timeOfDay;
        RecalculateImmediate("day_started");
    }

    public void OnTimeChanged(int newTime)
    {
        if (!_config.Comfort.Enabled) return;

        var elapsed = GameTimeHelper.MinutesBetween(_lastTick, newTime);
        if (elapsed < TickIntervalMinutes) return;

        _lastTick = newTime;
        ApplyPerHourModifiers(elapsed);
    }

    public void OnSaveLoaded()
    {
        _lastTick = GameHelper.IsWorldReady()
            ? Game1.timeOfDay
            : 0;
        RecalculateImmediate("save_loaded");
    }

    public void ApplyDirectAdjustment(float delta, string reason)
    {
        if (!_config.Comfort.Enabled) return;

        var state = _stateManager.GetCurrentState();
        if (state is null) return;

        var newComfort = Math.Clamp(
            state.Comfort.CurrentComfort + delta,
            0f,
            _config.Comfort.MaxComfort);

        _stateManager.UpdateComfort(newComfort, reason);

        _log.Debug(
            "Comfort direct adjustment: {0:+0.0;-0.0} ({1}). New: {2:F1}.",
            delta, reason, newComfort);
    }

    private void OnDiaperStateChanged(DiaperStateChangedEventArgs e)
    {
        ApplyImmediateModifiers("diaper_state_changed");
    }

    private void OnRegressionChanged(RegressionChangedEventArgs e)
    {
        ApplyImmediateModifiers("regression_changed");
    }

    private void OnContinenceThresholdCrossed(
        ContinenceThresholdCrossedEventArgs e)
    {
        ApplyImmediateModifiers("continence_threshold_crossed");
    }

    private void OnNeedsValueChanged(NeedsValueChangedEventArgs e)
    {
        if (e.NeedId is not ("hunger" or "thirst"))
            return;

        var delta = Math.Abs(e.PreviousNormalized - e.NewNormalized);
        if (delta >= 0.1f)
            ApplyImmediateModifiers($"{e.NeedId}_changed");
    }

    private void OnFurnitureProximityChanged(
        FurnitureProximityChangedEventArgs e)
    {
        ApplyImmediateModifiers("furniture_proximity_changed");
    }

    private void OnDiaperComfortEffect(DiaperComfortEffectEventArgs e)
    {
        ApplyDirectAdjustment(e.Delta, e.Reason);
    }

    private void ApplyImmediateModifiers(string reason)
    {
        if (!_config.Comfort.Enabled) return;

        var state = _stateManager.GetCurrentState();
        if (state is null) return;

        var context = BuildComfortContext(state);
        var previousActive = new HashSet<string>(state.Comfort.ActiveModifiers.Keys);
        var newActive = new HashSet<string>();
        var immediateTotal = 0f;

        foreach (var modifier in _dataLoader.ComfortModifiers
                     .OrderByDescending(m => m.Priority))
        {
            if (!_conditionEvaluator.Evaluate(modifier.Conditions, context))
                continue;

            newActive.Add(modifier.Id);

            if (!previousActive.Contains(modifier.Id) &&
                modifier.ImmediateValue != 0f)
            {
                immediateTotal += modifier.ImmediateValue;
                _log.Trace("Comfort immediate: {0} = {1:+0.0;-0.0}.",
                    modifier.Id, modifier.ImmediateValue);
            }
        }

        state.Comfort.ActiveModifiers = newActive
            .ToDictionary(
                id => id,
                id => _dataLoader.ComfortModifiers
                    .FirstOrDefault(m => m.Id == id)?.ValuePerHour ?? 0f);

        if (Math.Abs(immediateTotal) > 0.001f)
        {
            var newComfort = Math.Clamp(
                state.Comfort.CurrentComfort + immediateTotal,
                0f,
                _config.Comfort.MaxComfort);

            _stateManager.UpdateComfort(newComfort, reason);
        }
    }

    private void ApplyPerHourModifiers(int elapsedMinutes)
    {
        var state = _stateManager.GetCurrentState();
        if (state is null) return;

        var context = BuildComfortContext(state);
        var totalPerHour = 0f;

        foreach (var modifier in _dataLoader.ComfortModifiers)
        {
            if (_conditionEvaluator.Evaluate(modifier.Conditions, context))
                totalPerHour += modifier.ValuePerHour;
        }

        if (Math.Abs(totalPerHour) < 0.001f) return;

        var hours = elapsedMinutes / 60f;
        var delta = totalPerHour * hours;
        var newComfort = Math.Clamp(
            state.Comfort.CurrentComfort + delta,
            0f,
            _config.Comfort.MaxComfort);

        _stateManager.UpdateComfort(newComfort, "hourly_tick");

        _log.Trace(
            "Comfort tick: {0:+0.0;-0.0}/hr × {1:F2}hr = {2:+0.0;-0.0}. " +
            "New: {3:F1}.",
            totalPerHour, hours, delta, newComfort);
    }

    private void RecalculateImmediate(string reason)
    {
        if (!_config.Comfort.Enabled) return;

        var state = _stateManager.GetCurrentState();
        if (state is null) return;

        state.Comfort.ActiveModifiers.Clear();
        ApplyImmediateModifiers(reason);
    }

    private DialogueContext BuildComfortContext(PlayerRegressionState state)
    {
        var stage = _dataLoader.GetStage(state.CurrentStageId);

        return new DialogueContext
        {
            RegressionStageId = state.CurrentStageId,
            RegressionStageOrder = stage?.Order ?? 0,
            DiaperConditionId = state.Diaper.IsWearingDiaper
                ? state.Diaper.ConditionId : "none",
            IsWearingDiaper = state.Diaper.IsWearingDiaper,
            ContinenceNormalized = state.Needs.Continence.Value.Normalized,
            ContinenceThresholdId =
                state.Needs.Continence.Value.LastKnownThresholdId,
            HungerNormalized = state.Needs.Hunger.Normalized,
            ThirstNormalized = state.Needs.Thirst.Normalized,
            ComfortNormalized = state.Comfort.GetNormalized(
                _config.Comfort.MaxComfort),
            EquippedAccessories = state.EquippedAccessories,
            NpcName = string.Empty,
            FriendshipHearts = 0,
            IsMarried = false,
            NpcPersonalityTags = Array.Empty<string>(),
            Season = GameHelper.GetCurrentSeason(),
            TimeOfDay = GameHelper.GetCurrentTime(),
            LocationName = GameHelper.GetCurrentLocationName(),
            Weather = GetCurrentWeather(),
            IsFestivalDay = false,
            GameFlags = new HashSet<string>()
        };
    }

    private static string GetCurrentWeather()
    {
        if (!GameHelper.IsWorldReady()) return "sunny";
        if (Game1.isLightning) return "stormy";
        if (Game1.isRaining) return "rainy";
        if (Game1.isSnowing) return "snowy";
        if (Game1.isDebrisWeather) return "windy";
        return "sunny";
    }
}
