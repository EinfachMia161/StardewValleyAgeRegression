using AgeRegression.Config;
using AgeRegression.Data;
using AgeRegression.Events;
using AgeRegression.State;
using AgeRegression.Utilities;

namespace AgeRegression.Systems;

/// <summary>
/// Single source of truth for all diaper state mutations.
///
/// <para>
/// <b>Dependency design:</b> This class has no reference to
/// <see cref="ComfortSystem"/>. Comfort effects from diaper interactions
/// are communicated by publishing
/// <see cref="DiaperComfortEffectEventArgs"/> on the
/// <see cref="ModEventBus"/>. <see cref="ComfortSystem"/> subscribes
/// and applies the effect independently.
/// </para>
///
/// <para>
/// <b>Two API tiers:</b>
/// <list type="bullet">
///   <item><b>Player interaction</b>
///   (<see cref="EquipWithEffects"/>,
///   <see cref="ChangeWithEffects"/>,
///   <see cref="RemoveWithEffects"/>): Updates state and publishes
///   comfort effect events. Called by
///   <see cref="Items.DiaperInteractionHandler"/>.</item>
///   <item><b>Programmatic</b>
///   (<see cref="TryEquipDiaper"/>, <see cref="ChangeDiaper"/>,
///   <see cref="RemoveDiaper"/>): Updates state only. No comfort
///   effects. Called by event scripts, NPC systems, and future
///   APIs.</item>
/// </list>
/// </para>
/// </summary>
public sealed class DiaperSystem
{
    private readonly StateManager _stateManager;
    private readonly DataLoader _dataLoader;
    private readonly ModConfig _config;
    private readonly ModEventBus _eventBus;
    private readonly LogHelper _log;

    private int _lastWetnessTick = 0;

    public DiaperSystem(
        StateManager stateManager,
        DataLoader dataLoader,
        ModConfig config,
        ModEventBus eventBus,
        LogHelper log)
    {
        _stateManager = stateManager;
        _dataLoader   = dataLoader;
        _config       = config;
        _eventBus     = eventBus;
        _log          = log;
    }

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    /// <summary>Called at the start of each in-game day.</summary>
    public void OnDayStarted()
    {
        _lastWetnessTick = StardewValley.Game1.timeOfDay;
    }

    /// <summary>Called each time the in-game clock advances.</summary>
    public void OnTimeChanged(int newTime)
    {
        if (!_config.Enabled || !_config.Diapers.WetnessEnabled)
            return;

        var state = _stateManager.GetCurrentState();
        if (state is null || !state.Diaper.IsWearingDiaper)
            return;

        var elapsed = GameTimeHelper.MinutesBetween(_lastWetnessTick, newTime);
        if (elapsed < _config.Diapers.WetnessTickIntervalMinutes)
            return;

        _lastWetnessTick = newTime;
        ProcessWetnessTick(state);
    }

    // -------------------------------------------------------------------------
    // Player interaction API (state + comfort effect events)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Equips a diaper and publishes a
    /// <see cref="DiaperComfortEffectEventArgs"/> carrying the equip
    /// comfort bonus from
    /// <see cref="DiaperTypeData.EquipComfortBonus"/>.
    /// </summary>
    public bool EquipWithEffects(string diaperTypeId, bool hasBooster = false)
    {
        var typeData = _dataLoader.GetDiaperType(diaperTypeId);
        if (typeData is null)
        {
            _log.Warn("EquipWithEffects: unknown diaper type '{0}'.",
                diaperTypeId);
            return false;
        }

        if (!TryEquipDiaper(diaperTypeId, hasBooster))
            return false;

        PublishComfortEffect(
            typeData.EquipComfortBonus,
            $"diaper_equipped_{typeData.Id}");

        _log.Info("Diaper equipped with effects: '{0}', comfort+{1:F1}.",
            typeData.Id, typeData.EquipComfortBonus);
        return true;
    }

    /// <summary>
    /// Changes the diaper and publishes a
    /// <see cref="DiaperComfortEffectEventArgs"/> carrying the change
    /// comfort bonus (<see cref="DiaperTypeData.EquipComfortBonus"/>
    /// × <see cref="DiaperTypeData.ChangeComfortBonusRatio"/>).
    /// </summary>
    public bool ChangeWithEffects(string newDiaperTypeId, bool hasBooster = false)
    {
        var typeData = _dataLoader.GetDiaperType(newDiaperTypeId);
        if (typeData is null)
        {
            _log.Warn("ChangeWithEffects: unknown diaper type '{0}'.",
                newDiaperTypeId);
            return false;
        }

        var previousTypeId = _stateManager.GetCurrentState()
            ?.Diaper.EquippedDiaperTypeId;

        if (!ChangeDiaper(newDiaperTypeId, hasBooster))
            return false;

        var changeBonus =
            typeData.EquipComfortBonus * typeData.ChangeComfortBonusRatio;
        PublishComfortEffect(changeBonus, $"diaper_changed_to_{typeData.Id}");

        _log.Info(
            "Diaper changed with effects: '{0}' ? '{1}', comfort+{2:F1}.",
            previousTypeId ?? "none", typeData.Id, changeBonus);
        return true;
    }

    /// <summary>
    /// Removes the diaper and publishes a
    /// <see cref="DiaperComfortEffectEventArgs"/> carrying the remove
    /// comfort delta from
    /// <see cref="DiaperTypeData.RemoveComfortDelta"/>.
    /// </summary>
    public bool RemoveWithEffects()
    {
        var state = _stateManager.GetCurrentState();
        if (state is null || !state.Diaper.IsWearingDiaper)
        {
            _log.Debug("RemoveWithEffects: no diaper equipped.");
            return false;
        }

        var typeData = _dataLoader.GetDiaperType(
            state.Diaper.EquippedDiaperTypeId!);

        if (!RemoveDiaper())
            return false;

        if (typeData is not null &&
            Math.Abs(typeData.RemoveComfortDelta) > 0.001f)
        {
            PublishComfortEffect(
                typeData.RemoveComfortDelta,
                $"diaper_removed_{typeData.Id}");
        }

        _log.Info(
            "Diaper removed with effects: '{0}', comfort{1:+0.0;-0.0}.",
            typeData?.Id ?? "unknown",
            typeData?.RemoveComfortDelta ?? 0f);
        return true;
    }

    // -------------------------------------------------------------------------
    // Programmatic API (state changes only, no comfort effects)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Equips a diaper without publishing comfort effects.
    /// Use for programmatic calls from event scripts, NPC systems,
    /// and APIs.
    /// </summary>
    public bool TryEquipDiaper(string diaperTypeId, bool hasBooster = false)
    {
        var typeData = _dataLoader.GetDiaperType(diaperTypeId);
        if (typeData is null)
        {
            _log.Warn("TryEquipDiaper: unknown diaper type '{0}'.",
                diaperTypeId);
            return false;
        }

        var absoluteDay = AbsoluteDayHelper.GetCurrentAbsoluteDay();
        var newState    = DiaperState.CreateFresh(
            diaperTypeId, hasBooster, absoluteDay);

        _stateManager.UpdateDiaperState(newState);
        _log.Debug("Diaper equipped (programmatic): '{0}', booster={1}.",
            diaperTypeId, hasBooster);
        return true;
    }

    /// <summary>
    /// Changes the diaper without publishing comfort effects.
    /// Replaces the current diaper with a fresh one of the specified
    /// type.
    /// </summary>
    public bool ChangeDiaper(string newDiaperTypeId, bool hasBooster = false)
    {
        var state = _stateManager.GetCurrentState();
        if (state is null)
        {
            _log.Warn("ChangeDiaper: state not loaded.");
            return false;
        }

        var typeData = _dataLoader.GetDiaperType(newDiaperTypeId);
        if (typeData is null)
        {
            _log.Warn("ChangeDiaper: unknown diaper type '{0}'.",
                newDiaperTypeId);
            return false;
        }

        var absoluteDay  = AbsoluteDayHelper.GetCurrentAbsoluteDay();
        var changedState = DiaperState.CreateFresh(
            newDiaperTypeId, hasBooster, absoluteDay);

        _stateManager.UpdateDiaperState(changedState);
        _log.Debug("Diaper changed (programmatic): ? '{0}', booster={1}.",
            newDiaperTypeId, hasBooster);
        return true;
    }

    /// <summary>
    /// Removes the currently equipped diaper without publishing comfort
    /// effects.
    /// </summary>
    public bool RemoveDiaper()
    {
        var state = _stateManager.GetCurrentState();
        if (state is null || !state.Diaper.IsWearingDiaper)
        {
            _log.Debug("RemoveDiaper: no diaper equipped.");
            return false;
        }

        _stateManager.UpdateDiaperState(DiaperState.None);
        _log.Debug("Diaper removed (programmatic).");
        return true;
    }

    // -------------------------------------------------------------------------
    // Wetness tick (internal)
    // -------------------------------------------------------------------------

    private void ProcessWetnessTick(PlayerRegressionState state)
    {
        var diaperType = _dataLoader.GetDiaperType(
            state.Diaper.EquippedDiaperTypeId!);

        if (diaperType is null)
        {
            _log.Warn("Wetness tick: diaper type '{0}' not found.",
                state.Diaper.EquippedDiaperTypeId);
            return;
        }

        if (_config.Diapers.AccidentsEnabled)
            EvaluateAccident(state, diaperType);
    }

    private void EvaluateAccident(
        PlayerRegressionState state,
        DiaperTypeData diaperType)
    {
        var baseline     = _dataLoader.GetBaselineStage();
        var currentStage = _dataLoader.GetStage(state.CurrentStageId);
        if (baseline is null || currentStage is null) return;

        var maxOrder = _dataLoader.Stages.Max(s => s.Order);

        var chance = DiaperCalculations.CalculateAccidentChance(
            state.Diaper,
            currentStage.Id,
            baseline.Id,
            currentStage.Order,
            maxOrder,
            _config.Diapers.BaseAccidentChance);

        if (chance <= 0f) return;

        var roll = (float)Random.Shared.NextDouble();
        if (roll > chance) return;

        var wetnessIncrease =
            DiaperCalculations.CalculateWetnessIncrease(diaperType);
        var newWetness = Math.Min(
            state.Diaper.WetnessLevel + wetnessIncrease, 1f);

        _stateManager.UpdateDiaperState(state.Diaper.WithWetness(newWetness));
        _stateManager.RecordAccident(AccidentType.Wetting);

        _log.Debug("Wetting accident. New wetness: {0:P0}.", newWetness);

        if (_config.Diapers.MessingEnabled)
            EvaluateMessingAccident(
                state, diaperType, baseline.Id, currentStage, maxOrder);
    }

    private void EvaluateMessingAccident(
        PlayerRegressionState state,
        DiaperTypeData diaperType,
        string baselineId,
        RegressionStageData currentStage,
        int maxOrder)
    {
        var baseChance = DiaperCalculations.CalculateAccidentChance(
            state.Diaper,
            currentStage.Id,
            baselineId,
            currentStage.Order,
            maxOrder,
            _config.Diapers.BaseAccidentChance);

        var messChance =
            baseChance * _config.Diapers.MessingAccidentChanceMultiplier;
        if (messChance <= 0f) return;

        if ((float)Random.Shared.NextDouble() > messChance) return;

        var current = _stateManager.GetCurrentState();
        if (current is null) return;

        var newMessing = Math.Min(
            current.Diaper.MessingLevel
            + _config.Diapers.MessingIncreasePerAccident,
            1f);

        _stateManager.UpdateDiaperState(
            current.Diaper.WithMessing(newMessing));
        _stateManager.RecordAccident(AccidentType.Messing);

        _log.Debug("Messing accident. New messing: {0:P0}.", newMessing);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Publishes a <see cref="DiaperComfortEffectEventArgs"/> on the
    /// event bus. <see cref="ComfortSystem"/> subscribes and applies
    /// the effect.
    /// </summary>
    private void PublishComfortEffect(float delta, string reason)
    {
        if (Math.Abs(delta) < 0.001f) return;

        var state = _stateManager.GetCurrentState();
        if (state is null) return;

        _eventBus.Publish(new DiaperComfortEffectEventArgs(
            delta, reason, state.PlayerId));
    }
}
