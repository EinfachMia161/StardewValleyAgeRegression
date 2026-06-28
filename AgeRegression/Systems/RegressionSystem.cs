using AgeRegression.Config;
using AgeRegression.Data;
using AgeRegression.Events;
using AgeRegression.State;
using AgeRegression.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace AgeRegression.Systems;

/// <summary>
/// Manages regression stage transitions and computes the player's
/// effective stat modifiers by composing contributions from all
/// registered <see cref="IStatModifierProvider"/> instances.
///
/// <para>
/// Stat modifier composition order:
/// <list type="number">
///   <item>Regression stage base modifiers (from data file)</item>
///   <item>Each registered <see cref="IStatModifierProvider"/> in
///   registration order</item>
/// </list>
/// All multipliers are composed multiplicatively. The final result is
/// clamped to [0.1, 2.0] for speed and energy, [0.1, 2.0] for XP.
/// </para>
/// </summary>
public sealed class RegressionSystem
{
    private readonly StateManager _stateManager;
    private readonly DataLoader _dataLoader;
    private readonly ModConfig _config;
    private readonly ModEventBus _eventBus;
    private readonly LogHelper _log;

    /// <summary>
    /// Registered stat modifier providers, in registration order.
    /// Iterated on every call to <see cref="GetEffectiveModifiers"/>.
    /// </summary>
    private readonly List<IStatModifierProvider> _modifierProviders = new();

    public RegressionSystem(
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
    // Provider registration
    // -------------------------------------------------------------------------

    /// <summary>
    /// Registers a stat modifier provider. Providers are evaluated in
    /// registration order. Call during mod initialization after all
    /// systems are constructed.
    ///
    /// <para>
    /// Built-in registration order (established in <c>ModEntry</c>):
    /// <list type="number">
    ///   <item>Mood (<see cref="MoodStatModifierProvider"/>)</item>
    ///   <item>Furniture proximity
    ///   (<see cref="FurnitureStatModifierProvider"/>)</item>
    ///   <item>Equipped items
    ///   (<see cref="Items.ItemStatModifierProvider"/>)</item>
    /// </list>
    /// </para>
    /// </summary>
    public void RegisterModifierProvider(IStatModifierProvider provider)
    {
        if (_modifierProviders.Any(p => p.ProviderId == provider.ProviderId))
        {
            _log.Warn(
                "Modifier provider '{0}' is already registered. " +
                "Skipping duplicate.",
                provider.ProviderId);
            return;
        }

        _modifierProviders.Add(provider);
        _log.Debug("Registered stat modifier provider: '{0}'.",
            provider.ProviderId);
    }

    // -------------------------------------------------------------------------
    // Stage transitions
    // -------------------------------------------------------------------------

    /// <summary>
    /// Attempts to regress the player one stage deeper.
    /// Respects <see cref="RegressionConfig.AllowManualRegression"/>.
    /// </summary>
    /// <returns><c>true</c> if the stage changed.</returns>
    public bool TryRegressOneStep()
    {
        if (!_config.Regression.AllowManualRegression)
        {
            _log.Debug("Manual regression is disabled in config.");
            return false;
        }

        var current = _stateManager.GetCurrentStage();
        if (current is null) return false;

        var next = _dataLoader.Stages
            .Where(s => s.Order > current.Order)
            .OrderBy(s => s.Order)
            .FirstOrDefault();

        if (next is null)
        {
            _log.Debug("Already at deepest regression stage '{0}'.",
                current.Id);
            return false;
        }

        return _stateManager.TrySetStage(next.Id);
    }

    /// <summary>
    /// Attempts to progress the player one stage shallower (toward
    /// baseline). Respects
    /// <see cref="RegressionConfig.AllowManualRegression"/>.
    /// </summary>
    /// <returns><c>true</c> if the stage changed.</returns>
    public bool TryProgressOneStep()
    {
        if (!_config.Regression.AllowManualRegression)
        {
            _log.Debug("Manual regression is disabled in config.");
            return false;
        }

        var current = _stateManager.GetCurrentStage();
        if (current is null) return false;

        var prev = _dataLoader.Stages
            .Where(s => s.Order < current.Order)
            .OrderByDescending(s => s.Order)
            .FirstOrDefault();

        if (prev is null)
        {
            _log.Debug("Already at baseline stage '{0}'.", current.Id);
            return false;
        }

        return _stateManager.TrySetStage(prev.Id);
    }

    /// <summary>
    /// Sets the player directly to the specified stage ID.
    /// Bypasses the manual regression config flag � intended for
    /// event scripts and external API callers.
    /// </summary>
    public bool TrySetStage(string stageId) =>
        _stateManager.TrySetStage(stageId);

    // -------------------------------------------------------------------------
    // Effective stat modifiers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the fully composed effective stat modifiers for the
    /// current player.
    ///
    /// <para>
    /// Composition:
    /// <list type="number">
    ///   <item>Start from the regression stage base modifiers.</item>
    ///   <item>Multiply each registered provider's contribution.</item>
    ///   <item>Clamp all multipliers to sane bounds.</item>
    /// </list>
    /// Returns <see cref="EffectiveStatModifiers.Identity"/> when the
    /// mod is disabled, stat modifiers are disabled, or no save is
    /// loaded.
    /// </para>
    /// </summary>
    public EffectiveStatModifiers GetEffectiveModifiers()
    {
        if (!_config.Enabled || !_config.Regression.StatModifiersEnabled)
            return EffectiveStatModifiers.Identity;

        var state = _stateManager.GetCurrentState();
        if (state is null)
            return EffectiveStatModifiers.Identity;

        var stage = _dataLoader.GetStage(state.CurrentStageId);
        if (stage is null)
            return EffectiveStatModifiers.Identity;

        // Start from stage base modifiers
        var speed       = stage.StatModifiers.SpeedMultiplier;
        var energy      = stage.StatModifiers.MaxEnergyMultiplier;
        var xp          = stage.StatModifiers.SkillXpMultiplier;
        var canUseTools = stage.StatModifiers.CanUseTools;

        // Stack each provider's contribution multiplicatively
        foreach (var provider in _modifierProviders)
        {
            StatModifierContribution contribution;
            try
            {
                contribution = provider.GetContribution();
            }
            catch (Exception ex)
            {
                _log.Exception(
                    $"Stat modifier provider '{provider.ProviderId}' " +
                    "threw an exception", ex);
                continue;
            }

            speed       *= contribution.SpeedMultiplier;
            energy      *= contribution.MaxEnergyMultiplier;
            xp          *= contribution.SkillXpMultiplier;
            canUseTools  = canUseTools && contribution.CanUseTools;
        }

        // Clamp to sane bounds
        speed  = Math.Clamp(speed,  0.1f, 2.0f);
        energy = Math.Clamp(energy, 0.1f, 2.0f);
        xp     = Math.Clamp(xp,    0.1f, 2.0f);

        return new EffectiveStatModifiers(speed, energy, xp, canUseTools);
    }

    // -------------------------------------------------------------------------
    // Queries
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns <c>true</c> if the player is at the baseline stage.
    /// </summary>
    public bool IsAtBaseline()
    {
        var state = _stateManager.GetCurrentState();
        if (state is null) return true;

        var baseline = _dataLoader.GetBaselineStage();
        return baseline is null || state.CurrentStageId == baseline.Id;
    }
}
