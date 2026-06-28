using AgeRegression.Config;
using AgeRegression.Data;
using AgeRegression.State;
using AgeRegression.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace AgeRegression.Systems;

/// <summary>
/// Manages equipping and removing wearable accessories (pacifiers,
/// bottles, mittens, bibs, onesies, plushies, etc.).
///
/// <para>
/// Each accessory occupies a named slot. Only one item per slot can be
/// equipped at a time. Equipping a new item into an occupied slot
/// automatically replaces the previous item.
/// </para>
/// </summary>
public sealed class WardrobeSystem
{
    private readonly StateManager _stateManager;
    private readonly DataLoader _dataLoader;
    private readonly ModConfig _config;
    private readonly LogHelper _log;

    public WardrobeSystem(
        StateManager stateManager,
        DataLoader dataLoader,
        ModConfig config,
        LogHelper log)
    {
        _stateManager = stateManager;
        _dataLoader   = dataLoader;
        _config       = config;
        _log          = log;
    }

    // -------------------------------------------------------------------------
    // Equip / unequip
    // -------------------------------------------------------------------------

    /// <summary>
    /// Attempts to equip the accessory with the given ID.
    /// If the slot is already occupied, the previous item is silently
    /// replaced.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the item was equipped successfully.
    /// </returns>
    public bool TryEquip(string itemId)
    {
        if (!_config.Enabled)
            return false;

        var item = _dataLoader.GetWardrobeItem(itemId);
        if (item is null)
        {
            _log.Warn("TryEquip: unknown wardrobe item '{0}'.", itemId);
            return false;
        }

        var state = _stateManager.GetCurrentState();
        if (state is null)
            return false;

        // Check minimum stage requirement
        if (item.MinStageOrder > 0)
        {
            var currentStage = _dataLoader.GetStage(state.CurrentStageId);
            if (currentStage is null || currentStage.Order < item.MinStageOrder)
            {
                _log.Debug(
                    "Cannot equip '{0}': requires stage order >= {1}, " +
                    "current is {2}.",
                    itemId, item.MinStageOrder, currentStage?.Order ?? -1);
                return false;
            }
        }

        // Remove any existing item in the same slot
        var displaced = GetEquippedItemInSlot(state, item.Slot);
        if (displaced is not null)
        {
            _stateManager.UnequipAccessory(displaced.Id);
            _log.Debug("Displaced '{0}' from slot '{1}'.",
                displaced.Id, item.Slot);
        }

        _stateManager.EquipAccessory(itemId);
        _log.Debug("Equipped '{0}' in slot '{1}'.", itemId, item.Slot);
        return true;
    }

    /// <summary>
    /// Removes the accessory with the given ID if it is currently
    /// equipped.
    /// </summary>
    /// <returns><c>true</c> if the item was removed.</returns>
    public bool TryUnequip(string itemId)
    {
        var state = _stateManager.GetCurrentState();
        if (state is null)
            return false;

        if (!_stateManager.UnequipAccessory(itemId))
        {
            _log.Debug("TryUnequip: '{0}' was not equipped.", itemId);
            return false;
        }

        _log.Debug("Unequipped '{0}'.", itemId);
        return true;
    }

    /// <summary>Removes all equipped accessories.</summary>
    public void UnequipAll()
    {
        var state = _stateManager.GetCurrentState();
        if (state is null) return;

        var count = state.EquippedAccessories.Count;
        foreach (var id in state.EquippedAccessories.ToList())
            _stateManager.UnequipAccessory(id);
        _log.Debug("Unequipped all accessories ({0} items).", count);
    }

    // -------------------------------------------------------------------------
    // Queries
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns all currently equipped wardrobe item definitions.
    /// Items whose IDs are no longer in the data file are silently
    /// skipped.
    /// </summary>
    public IEnumerable<WardrobeItemData> GetEquippedItems()
    {
        var state = _stateManager.GetCurrentState();
        if (state is null)
            yield break;

        foreach (var id in state.EquippedAccessories)
        {
            var item = _dataLoader.GetWardrobeItem(id);
            if (item is not null)
                yield return item;
        }
    }

    /// <summary>
    /// Returns <c>true</c> if the given item is currently equipped.
    /// </summary>
    public bool IsEquipped(string itemId)
    {
        var state = _stateManager.GetCurrentState();
        return state?.EquippedAccessories.Contains(itemId) ?? false;
    }

    /// <summary>
    /// Returns the currently equipped item in the given slot, or
    /// <c>null</c>.
    /// </summary>
    public WardrobeItemData? GetEquippedInSlot(string slot)
    {
        var state = _stateManager.GetCurrentState();
        if (state is null) return null;
        return GetEquippedItemInSlot(state, slot);
    }

    /// <summary>
    /// Returns the total comfort bonus from all equipped accessories.
    /// </summary>
    public float GetTotalComfortBonus()
    {
        return GetEquippedItems().Sum(item => item.ComfortBonus);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private WardrobeItemData? GetEquippedItemInSlot(
        PlayerRegressionState state,
        string slot)
    {
        foreach (var id in state.EquippedAccessories)
        {
            var item = _dataLoader.GetWardrobeItem(id);
            if (item is not null &&
                string.Equals(item.Slot, slot,
                    StringComparison.OrdinalIgnoreCase))
                return item;
        }

        return null;
    }
}
