using Newtonsoft.Json;

namespace AgeRegression.Data;

/// <summary>
/// Shared JSON settings for asset deserialization. Registers the
/// polymorphic <see cref="UnlockConditionJsonConverter"/> so the new
/// <c>Conditions</c> array format can be loaded from JSON.
///
/// <para>
/// Asset providers (<see cref="FileSystemAssetProvider"/> and the test
/// <c>InMemoryAssetProvider</c>) use these settings so the same converter
/// applies everywhere. Other serialization paths (save data) intentionally
/// do not use these settings.
/// </para>
/// </summary>
internal static class AssetJson
{
    public static readonly JsonSerializerSettings Settings = new()
    {
        Converters = { new UnlockConditionJsonConverter() }
    };
}
