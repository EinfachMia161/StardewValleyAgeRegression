using AgeRegression.Utilities;
using DataLoader = AgeRegression.Data.DataLoader;
using StardewValley;

namespace AgeRegression.Items;

/// <summary>
/// Creates mod item instances with correct initial <c>modData</c>
/// state. All item creation goes through this factory — callers never
/// construct items directly.
/// </summary>
public sealed class ItemFactory
{
    private readonly DataLoader _dataLoader;
    private readonly LogHelper _log;

    public ItemFactory(DataLoader dataLoader, LogHelper log)
    {
        _dataLoader = dataLoader;
        _log        = log;
    }

    // -------------------------------------------------------------------------
    // Diaper creation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a diaper item instance for the given type ID.
    /// Returns <c>null</c> if the type ID is unknown or item creation
    /// fails.
    /// </summary>
    /// <param name="diaperTypeId">
    /// Type ID from <c>diaper-types.json</c>.
    /// </param>
    /// <param name="hasBooster">
    /// Whether to include a booster insert.
    /// </param>
    /// <param name="currentAbsoluteDay">
    /// Current absolute day for change tracking.
    /// </param>
    public StardewValley.Object? CreateDiaper(
        string diaperTypeId,
        bool hasBooster = false,
        int currentAbsoluteDay = 0)
    {
        var typeData = _dataLoader.GetDiaperType(diaperTypeId);
        if (typeData is null)
        {
            _log.Warn("CreateDiaper: unknown diaper type '{0}'.",
                diaperTypeId);
            return null;
        }

        var qualifiedId = ItemIds.Object(
            $"Diaper_{ItemIds.CapitalizeId(diaperTypeId)}");

        var item = StardewValley.ItemRegistry
            .Create<StardewValley.Object>(qualifiedId);

        if (item is null)
        {
            _log.Warn(
                "CreateDiaper: ItemRegistry.Create returned null " +
                "for '{0}'.", qualifiedId);
            return null;
        }

        // Write initial state to modData
        item.modData[ItemIds.ModDataDiaperTypeId]   = diaperTypeId;
        item.modData[ItemIds.ModDataWetnessLevel]   = "0";
        item.modData[ItemIds.ModDataMessingLevel]   = "0";
        item.modData[ItemIds.ModDataHasBooster]     = hasBooster.ToString();
        item.modData[ItemIds.ModDataLastChangedDay] =
            currentAbsoluteDay.ToString();

        _log.Debug("Created diaper item '{0}' (booster={1}).",
            qualifiedId, hasBooster);
        return item;
    }

    // -------------------------------------------------------------------------
    // Accessory creation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates an accessory item instance for the given accessory ID.
    /// Returns <c>null</c> if the ID is unknown or item creation fails.
    /// </summary>
    public StardewValley.Object? CreateAccessory(string accessoryId)
    {
        var itemData = _dataLoader.GetWardrobeItem(accessoryId);
        if (itemData is null)
        {
            _log.Warn("CreateAccessory: unknown accessory '{0}'.",
                accessoryId);
            return null;
        }

        var qualifiedId = ItemIds.Object(
            $"Accessory_{ItemIds.CapitalizeId(accessoryId)}");

        var item = StardewValley.ItemRegistry
            .Create<StardewValley.Object>(qualifiedId);

        if (item is null)
        {
            _log.Warn(
                "CreateAccessory: ItemRegistry.Create returned null " +
                "for '{0}'.", qualifiedId);
            return null;
        }

        item.modData[ItemIds.ModDataAccessoryTypeId] = accessoryId;

        _log.Debug("Created accessory item '{0}'.", qualifiedId);
        return item;
    }
}
