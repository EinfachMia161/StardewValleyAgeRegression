using StardewValley.Mods;

namespace AgeRegression.Utilities;

/// <summary>
/// Typed helpers for reading and writing values in SMAPI's
/// <see cref="ModDataDictionary"/>. All keys are automatically
/// namespaced under <c>mia.AgeRegression/</c> to avoid collisions
/// with other mods.
/// </summary>
public static class ModDataHelper
{
    private const string KeyPrefix = "mia.AgeRegression/";

    // -------------------------------------------------------------------------
    // String
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads a string value, returning <paramref name="defaultValue"/>
    /// if the key is absent.
    /// </summary>
    public static string GetString(
        ModDataDictionary data,
        string key,
        string defaultValue = "")
    {
        return data.TryGetValue(KeyPrefix + key, out var value)
            ? value
            : defaultValue;
    }

    /// <summary>Writes a string value.</summary>
    public static void SetString(ModDataDictionary data, string key, string value)
    {
        data[KeyPrefix + key] = value;
    }

    // -------------------------------------------------------------------------
    // Int
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads an integer value, returning <paramref name="defaultValue"/>
    /// if the key is absent or the value cannot be parsed.
    /// </summary>
    public static int GetInt(
        ModDataDictionary data,
        string key,
        int defaultValue = 0)
    {
        if (data.TryGetValue(KeyPrefix + key, out var raw) &&
            int.TryParse(raw, out var parsed))
            return parsed;
        return defaultValue;
    }

    /// <summary>Writes an integer value.</summary>
    public static void SetInt(ModDataDictionary data, string key, int value)
    {
        data[KeyPrefix + key] = value.ToString();
    }

    // -------------------------------------------------------------------------
    // Float
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads a float value, returning <paramref name="defaultValue"/>
    /// if the key is absent or the value cannot be parsed.
    /// </summary>
    public static float GetFloat(
        ModDataDictionary data,
        string key,
        float defaultValue = 0f)
    {
        if (data.TryGetValue(KeyPrefix + key, out var raw) &&
            float.TryParse(
                raw,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var parsed))
            return parsed;
        return defaultValue;
    }

    /// <summary>Writes a float value using invariant culture.</summary>
    public static void SetFloat(ModDataDictionary data, string key, float value)
    {
        data[KeyPrefix + key] = value.ToString(
            System.Globalization.CultureInfo.InvariantCulture);
    }

    // -------------------------------------------------------------------------
    // Bool
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads a boolean value, returning <paramref name="defaultValue"/>
    /// if the key is absent or the value cannot be parsed.
    /// </summary>
    public static bool GetBool(
        ModDataDictionary data,
        string key,
        bool defaultValue = false)
    {
        if (data.TryGetValue(KeyPrefix + key, out var raw) &&
            bool.TryParse(raw, out var parsed))
            return parsed;
        return defaultValue;
    }

    /// <summary>Writes a boolean value.</summary>
    public static void SetBool(ModDataDictionary data, string key, bool value)
    {
        data[KeyPrefix + key] = value.ToString();
    }

    // -------------------------------------------------------------------------
    // Removal
    // -------------------------------------------------------------------------

    /// <summary>Removes a key from the dictionary if it exists.</summary>
    public static void Remove(ModDataDictionary data, string key)
    {
        data.Remove(KeyPrefix + key);
    }
}
