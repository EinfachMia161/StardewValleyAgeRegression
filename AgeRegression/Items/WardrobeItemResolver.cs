using AgeRegression.Data;

namespace AgeRegression.Items;

/// <summary>
/// Resolves a wardrobe item ID to its category and definition.
/// Has no dependency on Stardew APIs and does not create runtime objects.
/// </summary>
/// <remarks>
/// Pipeline: item id → <see cref="WardrobeItemResolver"/> → <see cref="ResolvedWardrobeItem"/>
/// </remarks>
public sealed class WardrobeItemResolver
{
    private readonly DataLoader _dataLoader;

    public WardrobeItemResolver(DataLoader dataLoader)
    {
        _dataLoader = dataLoader;
    }

    /// <summary>
    /// Resolves <paramref name="itemId"/> to a <see cref="ResolvedWardrobeItem"/>.
    /// Returns <c>null</c> if the ID is not found in any wardrobe category.
    /// </summary>
    public ResolvedWardrobeItem? Resolve(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return null;

        var diaper = _dataLoader.GetDiaperType(itemId);
        if (diaper is not null)
            return ResolvedWardrobeItem.FromDiaper(diaper);

        var accessory = _dataLoader.GetWardrobeItem(itemId);
        if (accessory is not null)
            return ResolvedWardrobeItem.FromAccessory(accessory);

        return null;
    }
}
