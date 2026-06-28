using AgeRegression.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Objects;
using DataLoader = AgeRegression.Data.DataLoader;

namespace AgeRegression.Items;

/// <summary>
/// Registers all mod items into Stardew Valley's <c>Data/Objects</c>
/// asset via SMAPI's <c>AssetRequested</c> event.
///
/// <para>
/// Item definitions are data-driven: the display name, description,
/// price, and sprite index come from <see cref="DiaperTypeData" /> and
/// <see cref="WardrobeItemData" />. No item properties are hardcoded
/// here.
/// </para>
///
/// <para>
/// Sprite sheets are expected at:
/// <c>assets/sprites/diapers.png</c> and
/// <c>assets/sprites/accessories.png</c>.
/// </para>
/// </summary>
public sealed class ItemRegistry
{
    private readonly DataLoader _dataLoader;
    private readonly IModHelper _helper;
    private readonly LogHelper _log;

    private const string DiaperSpritePath    = "assets/sprites/diapers.png";
    private const string AccessorySpritePath = "assets/sprites/accessories.png";

    public ItemRegistry(
        DataLoader dataLoader,
        IModHelper helper,
        LogHelper log)
    {
        _dataLoader = dataLoader;
        _helper     = helper;
        _log        = log;
    }

    /// <summary>
    /// Subscribes to SMAPI's <c>AssetRequested</c> event.
    /// Call once during <c>GameLaunched</c>.
    /// </summary>
    public void Register()
    {
        _helper.Events.Content.AssetRequested += OnAssetRequested;
        _log.Debug("ItemRegistry subscribed to AssetRequested.");
    }

    // -------------------------------------------------------------------------
    // Asset editing
    // -------------------------------------------------------------------------

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            e.Edit(ApplyObjectData, AssetEditPriority.Default);
    }

    private void ApplyObjectData(IAssetData asset)
    {
        var data = asset.AsDictionary<string, ObjectData>().Data;

        RegisterDiapers(data);
        RegisterAccessories(data);

        _log.Debug(
            "Registered {0} mod items into Data/Objects.",
            _dataLoader.DiaperTypes.Count + _dataLoader.WardrobeItems.Count);
    }

    private void RegisterDiapers(IDictionary<string, ObjectData> data)
    {
        foreach (var diaper in _dataLoader.DiaperTypes)
        {
            var key = ItemIds.ObjectKey(
                $"Diaper_{ItemIds.CapitalizeId(diaper.Id)}");

            data[key] = new ObjectData
            {
                Name        = key,
                DisplayName = diaper.DisplayName,
                Description = diaper.Description,
                Type        = "Basic",
                Category    = 0,
                Price       = diaper.Price,
                Texture     = _helper.ModContent
                    .GetInternalAssetName(DiaperSpritePath).Name,
                SpriteIndex = diaper.SpriteIndex,
                Edibility   = -300,
                CanBeGivenAsGift           = false,
                ExcludeFromShippingCollection = true,
                ExcludeFromRandomSale      = true,
                ContextTags = new List<string>
                {
                    "mia_age_regression",
                    "mia_diaper",
                    $"mia_diaper_{diaper.Id}"
                }
            };
        }
    }

    private void RegisterAccessories(IDictionary<string, ObjectData> data)
    {
        foreach (var accessory in _dataLoader.WardrobeItems)
        {
            var key = ItemIds.ObjectKey(
                $"Accessory_{ItemIds.CapitalizeId(accessory.Id)}");

            data[key] = new ObjectData
            {
                Name        = key,
                DisplayName = accessory.DisplayName,
                Description = accessory.Description,
                Type        = "Basic",
                Category    = 0,
                Price       = accessory.Price,
                Texture     = _helper.ModContent
                    .GetInternalAssetName(AccessorySpritePath).Name,
                SpriteIndex = accessory.SpriteIndex,
                Edibility   = -300,
                CanBeGivenAsGift           = false,
                ExcludeFromShippingCollection = true,
                ExcludeFromRandomSale      = true,
                ContextTags = new List<string>
                {
                    "mia_age_regression",
                    "mia_accessory",
                    $"mia_accessory_{accessory.Slot}"
                }
            };
        }
    }
}
