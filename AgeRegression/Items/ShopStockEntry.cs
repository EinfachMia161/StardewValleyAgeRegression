namespace AgeRegression.Items;

/// <summary>
/// A single purchasable entry returned by <see cref="ShopStockProvider"/>.
/// Pairs a resolved wardrobe item with the price to charge in a shop.
/// Does not hold any Stardew runtime objects.
/// </summary>
public sealed class ShopStockEntry
{
    public ResolvedWardrobeItem Item { get; }
    public int Price { get; }

    public ShopStockEntry(ResolvedWardrobeItem item, int price)
    {
        Item  = item;
        Price = price;
    }
}
