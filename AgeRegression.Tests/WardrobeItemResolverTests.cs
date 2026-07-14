using AgeRegression.Data;
using AgeRegression.Items;
using AgeRegression.Utilities;
using FluentAssertions;

namespace AgeRegression.Tests;

public sealed class WardrobeItemResolverTests
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static DataLoader BuildLoader(
        string? diaperJson = null,
        string? wardrobeJson = null)
    {
        var files = new Dictionary<string, string>();

        if (diaperJson is not null)
            files["assets/data/diaper-types.json"] = diaperJson;

        if (wardrobeJson is not null)
            files["assets/data/wardrobe-items.json"] = wardrobeJson;

        var loader = new DataLoader(new InMemoryAssetProvider(files), new LogHelper(NullMonitor.Instance));
        loader.LoadAll();
        return loader;
    }

    private static DataLoader BuildDefaultLoader()
    {
        const string diapers = """
            [
              { "Id": "basic_diaper",   "DisplayName": "Basic",   "SpriteSheet": "a.png", "SpriteIndex": 0 },
              { "Id": "premium_diaper", "DisplayName": "Premium", "SpriteSheet": "a.png", "SpriteIndex": 1 }
            ]
            """;

        const string accessories = """
            [
              { "Id": "pacifier",     "DisplayName": "Pacifier",     "Slot": "pacifier", "SpriteSheet": "b.png", "SpriteIndex": 0 },
              { "Id": "plushie_bunny","DisplayName": "Bunny Plushie","Slot": "plushie",  "SpriteSheet": "b.png", "SpriteIndex": 1 }
            ]
            """;

        return BuildLoader(diapers, accessories);
    }

    // -------------------------------------------------------------------------
    // Valid diaper IDs
    // -------------------------------------------------------------------------

    [Fact]
    public void Resolve_ValidDiaperId_ReturnsDiaperCategory()
    {
        var resolver = new WardrobeItemResolver(BuildDefaultLoader());

        var result = resolver.Resolve("basic_diaper");

        result.Should().NotBeNull();
        result!.Category.Should().Be(WardrobeCategory.Diaper);
    }

    [Fact]
    public void Resolve_ValidDiaperId_PopulatesDiaperDefinition()
    {
        var resolver = new WardrobeItemResolver(BuildDefaultLoader());

        var result = resolver.Resolve("premium_diaper");

        result!.DiaperDefinition.Should().NotBeNull();
        result.DiaperDefinition!.Id.Should().Be("premium_diaper");
        result.AccessoryDefinition.Should().BeNull();
    }

    [Fact]
    public void Resolve_ValidDiaperId_PreservesItemId()
    {
        var resolver = new WardrobeItemResolver(BuildDefaultLoader());

        var result = resolver.Resolve("basic_diaper");

        result!.ItemId.Should().Be("basic_diaper");
    }

    [Fact]
    public void Resolve_ValidDiaperId_PopulatesSpriteInfo()
    {
        var resolver = new WardrobeItemResolver(BuildDefaultLoader());

        var result = resolver.Resolve("premium_diaper");

        result!.SpriteSheet.Should().Be("a.png");
        result.SpriteIndex.Should().Be(1);
    }

    // -------------------------------------------------------------------------
    // Valid accessory IDs
    // -------------------------------------------------------------------------

    [Fact]
    public void Resolve_ValidAccessoryId_ReturnsAccessoryCategory()
    {
        var resolver = new WardrobeItemResolver(BuildDefaultLoader());

        var result = resolver.Resolve("pacifier");

        result.Should().NotBeNull();
        result!.Category.Should().Be(WardrobeCategory.Accessory);
    }

    [Fact]
    public void Resolve_ValidAccessoryId_PopulatesAccessoryDefinition()
    {
        var resolver = new WardrobeItemResolver(BuildDefaultLoader());

        var result = resolver.Resolve("plushie_bunny");

        result!.AccessoryDefinition.Should().NotBeNull();
        result.AccessoryDefinition!.Id.Should().Be("plushie_bunny");
        result.DiaperDefinition.Should().BeNull();
    }

    [Fact]
    public void Resolve_ValidAccessoryId_PreservesItemId()
    {
        var resolver = new WardrobeItemResolver(BuildDefaultLoader());

        var result = resolver.Resolve("pacifier");

        result!.ItemId.Should().Be("pacifier");
    }

    [Fact]
    public void Resolve_ValidAccessoryId_PopulatesSpriteInfo()
    {
        var resolver = new WardrobeItemResolver(BuildDefaultLoader());

        var result = resolver.Resolve("plushie_bunny");

        result!.SpriteSheet.Should().Be("b.png");
        result.SpriteIndex.Should().Be(1);
    }

    // -------------------------------------------------------------------------
    // Invalid IDs
    // -------------------------------------------------------------------------

    [Fact]
    public void Resolve_UnknownId_ReturnsNull()
    {
        var resolver = new WardrobeItemResolver(BuildDefaultLoader());

        resolver.Resolve("does_not_exist").Should().BeNull();
    }

    [Fact]
    public void Resolve_EmptyString_ReturnsNull()
    {
        var resolver = new WardrobeItemResolver(BuildDefaultLoader());

        resolver.Resolve(string.Empty).Should().BeNull();
    }

    [Fact]
    public void Resolve_WhitespaceString_ReturnsNull()
    {
        var resolver = new WardrobeItemResolver(BuildDefaultLoader());

        resolver.Resolve("   ").Should().BeNull();
    }

    [Fact]
    public void Resolve_WrongCategory_DoesNotCrossMatch()
    {
        // "pacifier" is an accessory — should not resolve as a diaper
        var resolver = new WardrobeItemResolver(BuildDefaultLoader());

        var result = resolver.Resolve("pacifier");

        result!.Category.Should().NotBe(WardrobeCategory.Diaper);
    }

    // -------------------------------------------------------------------------
    // Future categories can be added without changing command logic
    // -------------------------------------------------------------------------

    [Fact]
    public void Resolve_NewDiaperAddedToData_ResolvesWithoutCodeChange()
    {
        const string diapers = """
            [
              { "Id": "basic_diaper",  "DisplayName": "Basic",  "SpriteSheet": "a.png", "SpriteIndex": 0 },
              { "Id": "future_diaper", "DisplayName": "Future",  "SpriteSheet": "a.png", "SpriteIndex": 9 }
            ]
            """;

        var resolver = new WardrobeItemResolver(BuildLoader(diaperJson: diapers));

        var result = resolver.Resolve("future_diaper");

        result.Should().NotBeNull();
        result!.Category.Should().Be(WardrobeCategory.Diaper);
        result.DiaperDefinition!.Id.Should().Be("future_diaper");
    }

    [Fact]
    public void Resolve_NewAccessoryAddedToData_ResolvesWithoutCodeChange()
    {
        const string accessories = """
            [
              { "Id": "pacifier",       "DisplayName": "Pacifier",     "Slot": "pacifier",  "SpriteSheet": "b.png", "SpriteIndex": 0 },
              { "Id": "future_item",    "DisplayName": "Future Item",  "Slot": "new_slot",  "SpriteSheet": "b.png", "SpriteIndex": 9 }
            ]
            """;

        var resolver = new WardrobeItemResolver(BuildLoader(wardrobeJson: accessories));

        var result = resolver.Resolve("future_item");

        result.Should().NotBeNull();
        result!.Category.Should().Be(WardrobeCategory.Accessory);
        result.AccessoryDefinition!.Id.Should().Be("future_item");
    }

    // -------------------------------------------------------------------------
    // Empty data sets
    // -------------------------------------------------------------------------

    [Fact]
    public void Resolve_EmptyDataSets_ReturnsNull()
    {
        // DataLoader falls back to built-in defaults when files are missing,
        // but an explicit empty array means no items are registered.
        var loader   = BuildLoader(diaperJson: "[]", wardrobeJson: "[]");
        var resolver = new WardrobeItemResolver(loader);

        resolver.Resolve("basic_diaper").Should().BeNull();
        resolver.Resolve("pacifier").Should().BeNull();
    }
}
