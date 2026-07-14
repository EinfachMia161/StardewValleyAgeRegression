namespace AgeRegression.Data;

/// <summary>
/// Common contract for all wardrobe item definitions.
/// Implemented by <see cref="DiaperTypeData"/> and <see cref="WardrobeItemData"/>.
/// </summary>
/// <remarks>
/// Consumers that need only cross-cutting concerns (unlock gating, shop
/// eligibility, item registration, rendering) should depend on this
/// interface rather than the concrete types. Category-specific fields
/// remain on the concrete classes and are accessible via
/// <see cref="Items.ResolvedWardrobeItem.DiaperDefinition"/> and
/// <see cref="Items.ResolvedWardrobeItem.AccessoryDefinition"/>.
/// </remarks>
public interface IWardrobeItemDefinition
{
    string Id            { get; }
    string DisplayName   { get; }
    string Description   { get; }
    int    Price         { get; }
    bool   ShopAvailable { get; }

    /// <summary>
    /// Unlock condition. An empty <see cref="UnlockRequirement"/> means
    /// always unlocked.
    /// </summary>
    UnlockRequirement Unlock { get; }

    string SpriteSheet { get; }
    int    SpriteIndex { get; }
}
