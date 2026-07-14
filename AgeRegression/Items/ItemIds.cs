using System;
using System.Collections.Generic;
using System.Linq;

namespace AgeRegression.Items;

/// <summary>
/// Central registry of all qualified item ID constants used by this
/// mod. Format: <c>(O)mia.AgeRegression_{Category}_{Id}</c>
///
/// <para>
/// These constants are used when registering items in
/// <c>Data/Objects</c> and when creating item instances via
/// <c>StardewValley.ItemRegistry.Create</c>.
/// The unqualified ID (without the <c>(O)</c> prefix) is used as the
/// dictionary key in <c>Data/Objects</c>.
/// </para>
/// </summary>
public static class ItemIds
{
    private const string ModPrefix = "mia.AgeRegression";

    /// <summary>
    /// Builds a qualified object item ID for the given local ID.
    /// </summary>
    public static string Object(string localId) =>
        $"(O){ModPrefix}_{localId}";

    /// <summary>
    /// Builds an unqualified object ID (used as Data/Objects key).
    /// </summary>
    public static string ObjectKey(string localId) =>
        $"{ModPrefix}_{localId}";

    // -------------------------------------------------------------------------
    // Diaper item IDs
    // -------------------------------------------------------------------------

    /// <summary>Qualified item ID for the basic diaper.</summary>
    public static readonly string DiaperBasic = Object("Diaper_Basic");

    /// <summary>Qualified item ID for the premium diaper.</summary>
    public static readonly string DiaperPremium = Object("Diaper_Premium");

    /// <summary>Qualified item ID for the training diaper.</summary>
    public static readonly string DiaperTraining = Object("Diaper_Training");

    // -------------------------------------------------------------------------
    // Accessory item IDs
    // -------------------------------------------------------------------------

    public static readonly string Pacifier     = Object("Accessory_Pacifier");
    public static readonly string BabyBottle   = Object("Accessory_BabyBottle");
    public static readonly string Mittens      = Object("Accessory_Mittens");
    public static readonly string Bib          = Object("Accessory_Bib");
    public static readonly string PlushieBunny = Object("Accessory_PlushieBunny");

    // -------------------------------------------------------------------------
    // modData keys for dynamic item state
    // -------------------------------------------------------------------------

    /// <summary>
    /// Namespace prefix for all modData keys on mod items.
    /// </summary>
    private const string DataPrefix = "mia.AgeRegression/";

    /// <summary>
    /// modData key: diaper type ID (e.g. <c>"basic"</c>).
    /// </summary>
    public const string ModDataDiaperTypeId =
        DataPrefix + "DiaperTypeId";

    /// <summary>
    /// modData key: current wetness level (float 0–1).
    /// </summary>
    public const string ModDataWetnessLevel =
        DataPrefix + "WetnessLevel";

    /// <summary>
    /// modData key: current messing level (float 0–1).
    /// </summary>
    public const string ModDataMessingLevel =
        DataPrefix + "MessingLevel";

    /// <summary>
    /// modData key: whether a booster insert is present.
    /// </summary>
    public const string ModDataHasBooster =
        DataPrefix + "HasBooster";

    /// <summary>
    /// modData key: absolute day the diaper was last changed.
    /// </summary>
    public const string ModDataLastChangedDay =
        DataPrefix + "LastChangedDay";

    /// <summary>
    /// modData key: accessory item type ID.
    /// </summary>
    public const string ModDataAccessoryTypeId =
        DataPrefix + "AccessoryTypeId";

    // -------------------------------------------------------------------------
    // Internal helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Capitalizes each underscore-separated segment of an ID.
    /// <c>"basic"</c> → <c>"Basic"</c>,
    /// <c>"plushie_bunny"</c> → <c>"Plushie_Bunny"</c>.
    /// </summary>
    internal static string CapitalizeId(string id)
    {
        if (string.IsNullOrEmpty(id)) return id;
        return string.Join("_",
            id.Split('_').Select(part =>
                part.Length > 0
                    ? char.ToUpperInvariant(part[0]) + part[1..]
                    : part));
    }

    // -------------------------------------------------------------------------
    // Qualified ID parsing
    // -------------------------------------------------------------------------

    // Anchored prefixes — the infix must start immediately after the mod prefix
    // to prevent misclassifying items from other mods that happen to contain
    // "_Diaper_" or "_Accessory_" somewhere in their ID.
    private static readonly string DiaperPrefix    = $"(O){ModPrefix}_Diaper_";
    private static readonly string AccessoryPrefix = $"(O){ModPrefix}_Accessory_";

    /// <summary>
    /// Tries to extract a diaper type ID from a qualified item ID.
    /// <c>(O)mia.AgeRegression_Diaper_Premium</c> → <c>"premium"</c>.
    /// Returns <c>null</c> if the ID is not a mod diaper.
    /// </summary>
    public static string? TryParseDiaperTypeId(string qualifiedId)
    {
        if (!qualifiedId.StartsWith(DiaperPrefix, StringComparison.Ordinal))
            return null;
        var raw = qualifiedId[DiaperPrefix.Length..];
        return raw.Length > 0 ? raw.ToLowerInvariant() : null;
    }

    /// <summary>
    /// Tries to extract an accessory type ID from a qualified item ID.
    /// <c>(O)mia.AgeRegression_Accessory_Plushie_Bunny</c> → <c>"plushie_bunny"</c>.
    /// Returns <c>null</c> if the ID is not a mod accessory.
    /// </summary>
    public static string? TryParseAccessoryTypeId(string qualifiedId)
    {
        if (!qualifiedId.StartsWith(AccessoryPrefix, StringComparison.Ordinal))
            return null;
        var raw = qualifiedId[AccessoryPrefix.Length..];
        return raw.Length > 0 ? raw.ToLowerInvariant() : null;
    }

    /// <summary>
    /// Returns true if this item is a custom diaper or accessory item.
    /// </summary>
    public static bool IsCustomItem(StardewValley.Object obj) =>
        obj.modData.ContainsKey(ModDataDiaperTypeId) ||
        obj.modData.ContainsKey(ModDataAccessoryTypeId);

    /// <summary>
    /// Gets the type of custom item.
    /// </summary>
    public static CustomItemType GetCustomItemType(StardewValley.Object obj)
    {
        if (obj.modData.ContainsKey(ModDataDiaperTypeId))
            return CustomItemType.Diaper;
        if (obj.modData.ContainsKey(ModDataAccessoryTypeId))
            return CustomItemType.Accessory;
        return CustomItemType.None;
    }
}

/// <summary>
/// Types of custom items that need special rendering.
/// </summary>
public enum CustomItemType
{
    None,
    Diaper,
    Accessory
}
