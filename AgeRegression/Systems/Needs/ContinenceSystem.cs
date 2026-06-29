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
    }

    public void OnTimeChanged(int newTime)
    {
        if (!_config.Needs.Continence.Enabled) return;

        var elapsed = GameTimeHelper.MinutesBetween(_lastTick, newTime);
        if (elapsed < _config.Needs.Continence.TickIntervalMinutes) return;

        _lastTick = newTime;

        var state = _stateManager.GetCurrentState();
        if (state is null) return;

        var drain = _drainStrategy.ComputeDrain(state, elapsed);
        if (Math.Abs(drain) >= 0.0001f)
        {
            _stateManager.UpdateContinence(drain);

            _log.Trace("Continence tick: drain={0:F4}.", drain);
        }
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
}