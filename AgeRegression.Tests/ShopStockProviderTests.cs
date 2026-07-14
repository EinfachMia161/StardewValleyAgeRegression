using AgeRegression.Data;
using AgeRegression.Items;
using AgeRegression.Utilities;
using FluentAssertions;

namespace AgeRegression.Tests;

public sealed class ShopStockProviderTests
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    // Always-unlocked stub — ShopStockProviderTests focus on ShopAvailable
    // filtering and price propagation; unlock logic is tested separately
    // in ItemUnlockServiceTests.
    private static readonly ItemUnlockService AlwaysUnlocked =
        TestItemUnlockService.AlwaysUnlocked;

    private static DataLoader BuildLoader(string diaperJson, string wardrobeJson)
    {
        var files = new Dictionary<string, string>
        {
            ["assets/data/diaper-types.json"]  = diaperJson,
            ["assets/data/wardrobe-items.json"] = wardrobeJson
        };
        var loader = new DataLoader(new InMemoryAssetProvider(files), new LogHelper(NullMonitor.Instance));
        loader.LoadAll();
        return loader;
    }

    private static DataLoader BuildDiapersOnly(string diaperJson) =>
        BuildLoader(diaperJson, "[]");

    private static DataLoader BuildAccessoriesOnly(string wardrobeJson) =>
        BuildLoader("[]", wardrobeJson);

    private static ShopStockProvider Provider(DataLoader loader) =>
        new(loader, AlwaysUnlocked);

    // -------------------------------------------------------------------------
    // ShopAvailable filtering — diapers
    // -------------------------------------------------------------------------

    [Fact]
    public void GetAvailableStock_ExcludesDiaperWithShopAvailableFalse()
    {
        const string json = """
            [
              { "Id": "basic",   "Price": 50,  "ShopAvailable": true  },
              { "Id": "special", "Price": 200, "ShopAvailable": false }
            ]
            """;
        var provider =         Provider(BuildDiapersOnly(json));

        var stock = provider.GetAvailableStock().ToList();

        stock.Should().HaveCount(1);
        stock[0].Item.ItemId.Should().Be("basic");
    }

    [Fact]
    public void GetAvailableStock_IncludesDiaperWhenShopAvailableTrue()
    {
        const string json = """
            [
              { "Id": "basic", "Price": 50, "ShopAvailable": true }
            ]
            """;
        var provider =         Provider(BuildDiapersOnly(json));

        provider.GetAvailableStock().Should().HaveCount(1);
    }

    [Fact]
    public void GetAvailableStock_IncludesDiaperWhenShopAvailableAbsent()
    {
        // Field absent → defaults to true in DiaperTypeData
        const string json = """
            [
              { "Id": "basic", "Price": 50 }
            ]
            """;
        var provider =         Provider(BuildDiapersOnly(json));

        provider.GetAvailableStock().Should().HaveCount(1);
    }

    // -------------------------------------------------------------------------
    // ShopAvailable filtering — accessories
    // -------------------------------------------------------------------------

    [Fact]
    public void GetAvailableStock_ExcludesAccessoryWithShopAvailableFalse()
    {
        const string json = """
            [
              { "Id": "pacifier", "Slot": "pacifier", "Price": 30, "ShopAvailable": true  },
              { "Id": "mittens",  "Slot": "mittens",  "Price": 20, "ShopAvailable": false }
            ]
            """;
        var provider =         Provider(BuildAccessoriesOnly(json));

        var stock = provider.GetAvailableStock().ToList();

        stock.Should().HaveCount(1);
        stock[0].Item.ItemId.Should().Be("pacifier");
    }

    [Fact]
    public void GetAvailableStock_IncludesAccessoryWhenShopAvailableAbsent()
    {
        const string json = """
            [
              { "Id": "pacifier", "Slot": "pacifier", "Price": 30 }
            ]
            """;
        var provider =         Provider(BuildAccessoriesOnly(json));

        provider.GetAvailableStock().Should().HaveCount(1);
    }

    // -------------------------------------------------------------------------
    // Price propagation
    // -------------------------------------------------------------------------

    [Fact]
    public void GetAvailableStock_PropagatesDiaperPrice()
    {
        const string json = """
            [
              { "Id": "premium", "Price": 120, "ShopAvailable": true }
            ]
            """;
        var provider =         Provider(BuildDiapersOnly(json));

        var entry = provider.GetAvailableStock().Single();

        entry.Price.Should().Be(120);
    }

    [Fact]
    public void GetAvailableStock_PropagatesAccessoryPrice()
    {
        const string json = """
            [
              { "Id": "plushie_bunny", "Slot": "plushie", "Price": 50, "ShopAvailable": true }
            ]
            """;
        var provider =         Provider(BuildAccessoriesOnly(json));

        var entry = provider.GetAvailableStock().Single();

        entry.Price.Should().Be(50);
    }

    // -------------------------------------------------------------------------
    // Category correctness
    // -------------------------------------------------------------------------

    [Fact]
    public void GetAvailableStock_DiaperEntriesHaveDiaperCategory()
    {
        const string json = """
            [
              { "Id": "basic", "Price": 50, "ShopAvailable": true }
            ]
            """;
        var provider =         Provider(BuildDiapersOnly(json));

        var entry = provider.GetAvailableStock().Single();

        entry.Item.Category.Should().Be(WardrobeCategory.Diaper);
        entry.Item.DiaperDefinition.Should().NotBeNull();
        entry.Item.AccessoryDefinition.Should().BeNull();
    }

    [Fact]
    public void GetAvailableStock_AccessoryEntriesHaveAccessoryCategory()
    {
        const string json = """
            [
              { "Id": "pacifier", "Slot": "pacifier", "Price": 30, "ShopAvailable": true }
            ]
            """;
        var provider =         Provider(BuildAccessoriesOnly(json));

        var entry = provider.GetAvailableStock().Single();

        entry.Item.Category.Should().Be(WardrobeCategory.Accessory);
        entry.Item.AccessoryDefinition.Should().NotBeNull();
        entry.Item.DiaperDefinition.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // Mixed categories
    // -------------------------------------------------------------------------

    [Fact]
    public void GetAvailableStock_ReturnsBothCategoriesWhenAllAvailable()
    {
        const string diapers    = """[ { "Id": "basic",    "Price": 50, "ShopAvailable": true } ]""";
        const string accessories = """[ { "Id": "pacifier", "Slot": "pacifier", "Price": 30, "ShopAvailable": true } ]""";
        var provider =         Provider(BuildLoader(diapers, accessories));

        var stock = provider.GetAvailableStock().ToList();

        stock.Should().HaveCount(2);
        stock.Should().ContainSingle(e => e.Item.Category == WardrobeCategory.Diaper);
        stock.Should().ContainSingle(e => e.Item.Category == WardrobeCategory.Accessory);
    }

    [Fact]
    public void GetAvailableStock_FiltersAcrossBothCategories()
    {
        const string diapers    = """[ { "Id": "basic",    "Price": 50, "ShopAvailable": false } ]""";
        const string accessories = """[ { "Id": "pacifier", "Slot": "pacifier", "Price": 30, "ShopAvailable": true  } ]""";
        var provider =         Provider(BuildLoader(diapers, accessories));

        var stock = provider.GetAvailableStock().ToList();

        stock.Should().HaveCount(1);
        stock[0].Item.Category.Should().Be(WardrobeCategory.Accessory);
    }

    // -------------------------------------------------------------------------
    // Empty and all-excluded data
    // -------------------------------------------------------------------------

    [Fact]
    public void GetAvailableStock_EmptyData_ReturnsEmpty()
    {
        var provider =         Provider(BuildLoader("[]", "[]"));

        provider.GetAvailableStock().Should().BeEmpty();
    }

    [Fact]
    public void GetAvailableStock_AllExcluded_ReturnsEmpty()
    {
        const string diapers    = """[ { "Id": "basic",    "Price": 50, "ShopAvailable": false } ]""";
        const string accessories = """[ { "Id": "pacifier", "Slot": "pacifier", "Price": 30, "ShopAvailable": false } ]""";
        var provider =         Provider(BuildLoader(diapers, accessories));

        provider.GetAvailableStock().Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // Future category compatibility
    // -------------------------------------------------------------------------

    [Fact]
    public void GetAvailableStock_MultipleNewDiapersAddedToData_AllSurfaceAutomatically()
    {
        const string json = """
            [
              { "Id": "alpha",  "Price": 10, "ShopAvailable": true },
              { "Id": "beta",   "Price": 20, "ShopAvailable": true },
              { "Id": "gamma",  "Price": 30, "ShopAvailable": true },
              { "Id": "hidden", "Price": 99, "ShopAvailable": false }
            ]
            """;
        var provider =         Provider(BuildDiapersOnly(json));

        var stock = provider.GetAvailableStock().ToList();

        stock.Should().HaveCount(3);
        stock.Select(e => e.Item.ItemId).Should().BeEquivalentTo("alpha", "beta", "gamma");
    }

    [Fact]
    public void GetAvailableStock_ItemIdMatchesDefinitionId()
    {
        const string json = """
            [ { "Id": "overnight", "Price": 150, "ShopAvailable": true } ]
            """;
        var provider =         Provider(BuildDiapersOnly(json));

        var entry = provider.GetAvailableStock().Single();

        entry.Item.ItemId.Should().Be("overnight");
        entry.Item.DiaperDefinition!.Id.Should().Be("overnight");
    }
}
