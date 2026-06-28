using AgeRegression.Data;
using AgeRegression.State;
using AgeRegression.Utilities;
using DataLoader = AgeRegression.Data.DataLoader;
using StardewValley;

namespace AgeRegression.Items;

/// <summary>
/// Reads and writes dynamic diaper state between a
/// <see cref="StardewValley.Object" />'s <c>modData</c> and the mod's
/// <see cref="DiaperState" /> model.
///
/// <para>
/// This handler is the single point of truth for the modData key
/// schema on diaper items. No other class reads or writes diaper item
/// modData directly.
/// </para>
/// </summary>
public sealed class DiaperItemHandler
{
    private readonly DataLoader _dataLoader;
    private readonly LogHelper _log;

    public DiaperItemHandler(DataLoader dataLoader, LogHelper log)
    {
        _dataLoader = dataLoader;
        _log        = log;
    }

    // -------------------------------------------------------------------------
    // Identification
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns <c>true</c> if the given item is a mod diaper item.
    /// </summary>
    public static bool IsDiaperItem(Item item) =>
        item is StardewValley.Object obj &&
        obj.modData.ContainsKey(ItemIds.ModDataDiaperTypeId);

    // -------------------------------------------------------------------------
    // State reading
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads the current <see cref="DiaperState" /> from an item's
    /// modData. Returns <see cref="DiaperState.None" /> if the item is
    /// not a diaper.
    /// </summary>
    public DiaperState ReadState(StardewValley.Object item)
    {
        if (!item.modData.TryGetValue(
                ItemIds.ModDataDiaperTypeId, out var typeId) ||
            string.IsNullOrWhiteSpace(typeId))
            return DiaperState.None;

        var wetness     = ReadFloat(item, ItemIds.ModDataWetnessLevel, 0f);
        var messing     = ReadFloat(item, ItemIds.ModDataMessingLevel, 0f);
        var hasBooster  = ReadBool(item, ItemIds.ModDataHasBooster, false);
        var lastChanged = ReadInt(item, ItemIds.ModDataLastChangedDay, 0);

        return new DiaperState
        {
            EquippedDiaperTypeId   = typeId,
            WetnessLevel           = wetness,
            MessingLevel           = messing,
            HasBooster             = hasBooster,
            LastChangedAbsoluteDay = lastChanged
        };
    }

    // -------------------------------------------------------------------------
    // State writing
    // -------------------------------------------------------------------------

    /// <summary>
    /// Writes a <see cref="DiaperState" /> into an item's modData.
    /// </summary>
    public void WriteState(StardewValley.Object item, DiaperState state)
    {
        if (state.EquippedDiaperTypeId is null)
        {
            _log.Warn("WriteState called with DiaperState.None — no-op.");
            return;
        }

        item.modData[ItemIds.ModDataDiaperTypeId] =
            state.EquippedDiaperTypeId;
        item.modData[ItemIds.ModDataWetnessLevel] =
            state.WetnessLevel.ToString(
                System.Globalization.CultureInfo.InvariantCulture);
        item.modData[ItemIds.ModDataMessingLevel] =
            state.MessingLevel.ToString(
                System.Globalization.CultureInfo.InvariantCulture);
        item.modData[ItemIds.ModDataHasBooster] =
            state.HasBooster.ToString();
        item.modData[ItemIds.ModDataLastChangedDay] =
            state.LastChangedAbsoluteDay.ToString();
    }

    // -------------------------------------------------------------------------
    // Display name update
    // -------------------------------------------------------------------------

    /// <summary>
    /// Updates the item's display name to reflect its current condition.
    /// Called after state changes so the inventory shows the correct
    /// name.
    /// </summary>
    public void UpdateDisplayName(
        StardewValley.Object item,
        DiaperState state)
    {
        var typeData = _dataLoader.GetDiaperType(
            state.EquippedDiaperTypeId ?? string.Empty);
        if (typeData is null) return;

        var conditionSuffix = state.ConditionId switch
        {
            "clean"  => string.Empty,
            "damp"   => " (Damp)",
            "wet"    => " (Wet)",
            "soaked" => " (Soaked)",
            _        => string.Empty
        };

        item.displayName = typeData.DisplayName + conditionSuffix;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static float ReadFloat(
        StardewValley.Object item,
        string key,
        float defaultValue)
    {
        if (item.modData.TryGetValue(key, out var raw) &&
            float.TryParse(
                raw,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var parsed))
            return parsed;
        return defaultValue;
    }

    private static int ReadInt(
        StardewValley.Object item,
        string key,
        int defaultValue)
    {
        if (item.modData.TryGetValue(key, out var raw) &&
            int.TryParse(raw, out var parsed))
            return parsed;
        return defaultValue;
    }

    private static bool ReadBool(
        StardewValley.Object item,
        string key,
        bool defaultValue)
    {
        if (item.modData.TryGetValue(key, out var raw) &&
            bool.TryParse(raw, out var parsed))
            return parsed;
        return defaultValue;
    }
}
