using AgeRegression.Config;
using AgeRegression.Data;
using AgeRegression.State;
using AgeRegression.Systems;
using AgeRegression.Utilities;

namespace AgeRegression.Items;

/// <summary>
/// Routes player accessory item interactions to
/// <see cref="WardrobeSystem" /> and applies comfort effects via
/// <see cref="ComfortSystem" />.
///
/// <para>
/// Interaction logic:
/// <list type="bullet">
///   <item>If the accessory is not equipped: equip it and apply the
///   <see cref="WardrobeItemData.ComfortBonus" /> as an immediate
///   comfort adjustment.</item>
///   <item>If the accessory is already equipped: unequip it and apply
///   the negative of the comfort bonus.</item>
/// </list>
/// </para>
///
/// <para>
/// Stat modifier effects (speed, etc.) are handled automatically by
/// <see cref="ItemStatModifierProvider" /> which reads
/// <see cref="WardrobeItemData.SpeedModifier" /> from the equipped set.
/// No additional stat wiring is needed here.
/// </para>
/// </summary>
public sealed class AccessoryInteractionHandler
{
    private readonly WardrobeSystem _wardrobeSystem;
    private readonly ComfortSystem _comfortSystem;
    private readonly DataLoader _dataLoader;
    private readonly StateManager _stateManager;
    private readonly ModConfig _config;
    private readonly LogHelper _log;

    public AccessoryInteractionHandler(
        WardrobeSystem wardrobeSystem,
        ComfortSystem comfortSystem,
        DataLoader dataLoader,
        StateManager stateManager,
        ModConfig config,
        LogHelper log)
    {
        _wardrobeSystem = wardrobeSystem;
        _comfortSystem  = comfortSystem;
        _dataLoader     = dataLoader;
        _stateManager   = stateManager;
        _config         = config;
        _log            = log;
    }

    // -------------------------------------------------------------------------
    // Public interaction entry points
    // -------------------------------------------------------------------------

    /// <summary>
    /// Handles a player using an accessory item from their inventory.
    /// Toggles equip state: equips if not equipped, unequips if already
    /// equipped.
    /// </summary>
    /// <param name="accessoryId">
    /// The accessory ID from <c>wardrobe-items.json</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the interaction was handled (suppresses vanilla
    /// behavior).
    /// </returns>
    public bool HandleUseItem(string accessoryId)
    {
        if (!_config.Enabled) return false;

        var itemData = _dataLoader.GetWardrobeItem(accessoryId);
        if (itemData is null)
        {
            _log.Warn("HandleUseItem: unknown accessory '{0}'.",
                accessoryId);
            return false;
        }

        if (_wardrobeSystem.IsEquipped(accessoryId))
            return HandleUnequip(accessoryId, itemData);

        return HandleEquip(accessoryId, itemData);
    }

    /// <summary>
    /// Explicitly equips an accessory by ID.
    /// Applies the comfort bonus from
    /// <see cref="WardrobeItemData.ComfortBonus" />.
    /// </summary>
    public bool HandleEquip(string accessoryId)
    {
        var itemData = _dataLoader.GetWardrobeItem(accessoryId);
        if (itemData is null)
        {
            _log.Warn("HandleEquip: unknown accessory '{0}'.",
                accessoryId);
            return false;
        }

        return HandleEquip(accessoryId, itemData);
    }

    /// <summary>
    /// Explicitly unequips an accessory by ID.
    /// Removes the comfort bonus that was applied on equip.
    /// </summary>
    public bool HandleUnequip(string accessoryId)
    {
        var itemData = _dataLoader.GetWardrobeItem(accessoryId);
        if (itemData is null)
        {
            _log.Warn("HandleUnequip: unknown accessory '{0}'.",
                accessoryId);
            return false;
        }

        return HandleUnequip(accessoryId, itemData);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private bool HandleEquip(string accessoryId, WardrobeItemData itemData)
    {
        if (!_wardrobeSystem.TryEquip(accessoryId))
        {
            _log.Debug(
                "HandleEquip: WardrobeSystem rejected equip of '{0}'.",
                accessoryId);
            return false;
        }

        if (itemData.ComfortBonus > 0.001f)
        {
            _comfortSystem.ApplyDirectAdjustment(
                itemData.ComfortBonus,
                $"accessory_equipped_{accessoryId}");
        }

        _log.Info(
            "Accessory equipped: '{0}' (slot='{1}'), comfort+{2:F1}.",
            accessoryId, itemData.Slot, itemData.ComfortBonus);
        return true;
    }

    private bool HandleUnequip(string accessoryId, WardrobeItemData itemData)
    {
        if (!_wardrobeSystem.TryUnequip(accessoryId))
        {
            _log.Debug(
                "HandleUnequip: WardrobeSystem rejected unequip of '{0}'.",
                accessoryId);
            return false;
        }

        if (itemData.ComfortBonus > 0.001f)
        {
            _comfortSystem.ApplyDirectAdjustment(
                -itemData.ComfortBonus,
                $"accessory_unequipped_{accessoryId}");
        }

        _log.Info(
            "Accessory unequipped: '{0}' (slot='{1}'), " +
            "comfort{2:+0.0;-0.0}.",
            accessoryId, itemData.Slot, -itemData.ComfortBonus);
        return true;
    }
}
