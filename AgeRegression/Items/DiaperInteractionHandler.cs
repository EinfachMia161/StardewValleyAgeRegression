using AgeRegression.Config;
using AgeRegression.State;
using AgeRegression.Systems;
using AgeRegression.Utilities;

namespace AgeRegression.Items;

/// <summary>
/// Routes player diaper item interactions to <see cref="DiaperSystem" />.
///
/// <para>
/// This class is intentionally thin. It is responsible only for:
/// <list type="bullet">
///   <item>Reading item identity from <c>modData</c>.</item>
///   <item>Determining whether the interaction is an equip or a
///   change.</item>
///   <item>Delegating to the appropriate
///   <see cref="DiaperSystem" /> method.</item>
/// </list>
/// All state mutation, comfort application, and event firing happen in
/// <see cref="DiaperSystem" />. This class contains no business logic.
/// </para>
/// </summary>
public sealed class DiaperInteractionHandler
{
    private readonly DiaperSystem _diaperSystem;
    private readonly StateManager _stateManager;
    private readonly ModConfig _config;
    private readonly LogHelper _log;

    public DiaperInteractionHandler(
        DiaperSystem diaperSystem,
        StateManager stateManager,
        ModConfig config,
        LogHelper log)
    {
        _diaperSystem = diaperSystem;
        _stateManager = stateManager;
        _config       = config;
        _log          = log;
    }

    // -------------------------------------------------------------------------
    // Public interaction entry points
    // -------------------------------------------------------------------------

    /// <summary>
    /// Handles a player using a diaper item from their inventory.
    /// Routes to <see cref="DiaperSystem.EquipWithEffects" /> or
    /// <see cref="DiaperSystem.ChangeWithEffects" /> depending on
    /// whether a diaper is already equipped.
    /// </summary>
    /// <param name="diaperTypeId">
    /// The diaper type ID from <c>diaper-types.json</c>.
    /// </param>
    /// <param name="hasBooster">
    /// Whether the item has a booster insert.
    /// </param>
    /// <returns>
    /// <c>true</c> if the interaction was handled (suppresses vanilla
    /// behavior).
    /// </returns>
    public bool HandleUseItem(string diaperTypeId, bool hasBooster = false)
    {
        if (!_config.Enabled) return false;

        var state = _stateManager.GetCurrentState();
        if (state is null) return false;

        if (state.Diaper.IsWearingDiaper)
        {
            _log.Debug(
                "Player used diaper '{0}' while wearing one — changing.",
                diaperTypeId);
            return _diaperSystem.ChangeWithEffects(diaperTypeId, hasBooster);
        }

        _log.Debug("Player used diaper '{0}' — equipping.", diaperTypeId);
        return _diaperSystem.EquipWithEffects(diaperTypeId, hasBooster);
    }

    /// <summary>
    /// Handles a player explicitly removing their diaper.
    /// Delegates to <see cref="DiaperSystem.RemoveWithEffects" />.
    /// </summary>
    /// <returns><c>true</c> if a diaper was removed.</returns>
    public bool HandleRemove()
    {
        if (!_config.Enabled) return false;
        return _diaperSystem.RemoveWithEffects();
    }
}
