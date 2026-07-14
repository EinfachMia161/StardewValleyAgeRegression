using AgeRegression.Items;
using AgeRegression.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Shops;

namespace AgeRegression.Shops;

/// <summary>
/// Stardew adapter that injects wardrobe items into Pierre's General Store.
///
/// <para>
/// This class contains all Stardew/SMAPI-specific logic.
/// It delegates all enumeration, filtering, and pricing decisions to
/// <see cref="ShopStockProvider"/>. It does not resolve item IDs,
/// calculate prices, or decide availability.
/// </para>
///
/// <para>
/// Pipeline:
/// Pierre Shop → <see cref="WardrobeShopIntegration"/>
///     → <see cref="ShopStockProvider"/> → <see cref="ShopStockEntry"/>
///     → <see cref="ItemFactory"/> → Purchased Item
/// </para>
/// </summary>
public sealed class WardrobeShopIntegration
{
    private const string PierreShopId = "Pierre";

    private readonly ShopStockProvider _stockProvider;
    private readonly IModHelper _helper;
    private readonly LogHelper _log;

    public WardrobeShopIntegration(
        ShopStockProvider stockProvider,
        IModHelper helper,
        LogHelper log)
    {
        _stockProvider = stockProvider;
        _helper        = helper;
        _log           = log;
    }

    /// <summary>
    /// Subscribes to SMAPI's <c>AssetRequested</c> event.
    /// Call once during mod entry.
    /// </summary>
    public void Register()
    {
        _helper.Events.Content.AssetRequested += OnAssetRequested;
        _log.Debug("WardrobeShopIntegration subscribed to AssetRequested.");
    }

    // -------------------------------------------------------------------------
    // SMAPI event handler
    // -------------------------------------------------------------------------

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
            e.Edit(InjectPierreStock, AssetEditPriority.Default);
    }

    // -------------------------------------------------------------------------
    // Adapter: ShopStockEntry → ShopItemData
    // -------------------------------------------------------------------------

    private void InjectPierreStock(IAssetData asset)
    {
        var shops = asset.AsDictionary<string, ShopData>().Data;

        if (!shops.TryGetValue(PierreShopId, out var pierre) || pierre is null)
        {
            _log.Warn("WardrobeShopIntegration: '{0}' shop not found in Data/Shops.",
                PierreShopId);
            return;
        }

        pierre.Items ??= new List<ShopItemData>();

        var added = 0;
        foreach (var entry in _stockProvider.GetAvailableStock())
        {
            var shopItem = ToShopItemData(entry);
            if (shopItem is null) continue;

            pierre.Items.Add(shopItem);
            added++;
        }

        _log.Debug("WardrobeShopIntegration: added {0} wardrobe item(s) to {1}.",
            added, PierreShopId);
    }

    /// <summary>
    /// Converts a <see cref="ShopStockEntry"/> to a <see cref="ShopItemData"/>.
    /// Returns <c>null</c> and logs a warning if the item cannot be mapped,
    /// so one bad entry never breaks the shop.
    /// </summary>
    private ShopItemData? ToShopItemData(ShopStockEntry entry)
    {
        try
        {
            var qualifiedId = entry.Item.Category switch
            {
                WardrobeCategory.Diaper =>
                    ItemIds.Object($"Diaper_{ItemIds.CapitalizeId(entry.Item.ItemId)}"),
                WardrobeCategory.Accessory =>
                    ItemIds.Object($"Accessory_{ItemIds.CapitalizeId(entry.Item.ItemId)}"),
                _ => null
            };

            if (qualifiedId is null)
            {
                _log.Warn(
                    "WardrobeShopIntegration: unhandled category '{0}' for item '{1}' — skipping.",
                    entry.Item.Category, entry.Item.ItemId);
                return null;
            }

            return new ShopItemData
            {
                Id             = $"mia.AgeRegression_{entry.Item.ItemId}",
                ItemId         = qualifiedId,
                Price          = entry.Price,
                AvailableStock = -1  // unlimited
            };
        }
        catch (Exception ex)
        {
            _log.Exception(
                $"WardrobeShopIntegration: failed to convert item '{entry.Item.ItemId}'",
                ex);
            return null;
        }
    }
}
