using AgeRegression.Data;
using AgeRegression.Utilities;
using DataLoader = AgeRegression.Data.DataLoader;
using StardewValley;

namespace AgeRegression.Items;

/// <summary>
/// Reads accessory item state from <c>modData</c> and provides helpers
/// for identifying mod accessory items.
/// </summary>
public sealed class AccessoryItemHandler
{
    private readonly DataLoader _dataLoader;
    private readonly LogHelper _log;

    public AccessoryItemHandler(DataLoader dataLoader, LogHelper log)
    {
        _dataLoader = dataLoader;
        _log        = log;
    }

    // -------------------------------------------------------------------------
    // Identification
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns <c>true</c> if the item is a mod accessory.
    /// </summary>
    public static bool IsAccessoryItem(Item item) =>
        item is StardewValley.Object obj &&
        obj.modData.ContainsKey(ItemIds.ModDataAccessoryTypeId);

    // -------------------------------------------------------------------------
    // State reading
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the accessory type ID stored in the item's modData,
    /// or <c>null</c> if the item is not a mod accessory.
    /// </summary>
    public static string? GetAccessoryTypeId(StardewValley.Object item) =>
        item.modData.TryGetValue(ItemIds.ModDataAccessoryTypeId, out var id)
            ? id
            : null;

    /// <summary>
    /// Returns the <see cref="WardrobeItemData" /> for the given item,
    /// or <c>null</c> if not found.
    /// </summary>
    public WardrobeItemData? GetItemData(StardewValley.Object item)
    {
        var typeId = GetAccessoryTypeId(item);
        return typeId is not null
            ? _dataLoader.GetWardrobeItem(typeId)
            : null;
    }
}
