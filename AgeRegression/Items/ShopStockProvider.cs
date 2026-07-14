using AgeRegression.Data;

namespace AgeRegression.Items;

/// <summary>
/// Enumerates purchasable wardrobe items from data definitions.
/// Has no dependency on Stardew APIs and does not create runtime objects.
/// </summary>
/// <remarks>
/// Pipeline:
/// Shop → WardrobeShopIntegration → <see cref="ShopStockProvider"/>
///     → <see cref="ItemUnlockService"/> → <see cref="ShopStockEntry"/>
///     → <see cref="ResolvedWardrobeItem"/> → <see cref="ItemFactory"/> → Purchased Item
///
/// The same provider is reusable by crafting, mail rewards, NPC gifts,
/// loot tables, and quests without modification.
/// </remarks>
public sealed class ShopStockProvider
{
    private readonly DataLoader _dataLoader;
    private readonly ItemUnlockService _unlockService;

    public ShopStockProvider(DataLoader dataLoader, ItemUnlockService unlockService)
    {
        _dataLoader     = dataLoader;
        _unlockService  = unlockService;
    }

    /// <summary>
    /// Returns all wardrobe items that are shop-eligible and currently unlocked.
    /// Items with <c>ShopAvailable = false</c> are excluded regardless of unlock state.
    /// Unlock conditions are evaluated by <see cref="ItemUnlockService"/>.
    /// Adding a new category to <see cref="DataLoader"/> automatically
    /// surfaces it here without any provider changes.
    /// </summary>
    public IEnumerable<ShopStockEntry> GetAvailableStock() =>
        StockFrom(_dataLoader.DiaperTypes.Select(ResolvedWardrobeItem.FromDiaper))
        .Concat(StockFrom(_dataLoader.WardrobeItems.Select(ResolvedWardrobeItem.FromAccessory)));

    private IEnumerable<ShopStockEntry> StockFrom(IEnumerable<ResolvedWardrobeItem> items)
    {
        foreach (var item in items)
        {
            if (!item.Definition.ShopAvailable) continue;
            if (!_unlockService.IsUnlocked(item)) continue;
            yield return new ShopStockEntry(item, item.Definition.Price);
        }
    }
}
