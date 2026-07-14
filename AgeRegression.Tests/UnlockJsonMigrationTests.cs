using System.IO;
using AgeRegression.Data;
using AgeRegression.Items;
using AgeRegression.Tests;
using static AgeRegression.Tests.UnlockTestHelpers;
using AgeRegression.Utilities;
using FluentAssertions;

namespace AgeRegression.Tests;

// ---------------------------------------------------------------------------
// Phase 5a/5b: production unlock JSON uses the new Conditions format
// ---------------------------------------------------------------------------

public sealed class UnlockJsonMigrationTests
{
    private static string FindModRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(
                   Path.Combine(dir.FullName, "AgeRegression", "assets", "data", "wardrobe-items.json")))
        {
            dir = dir.Parent;
        }

        if (dir is null)
            throw new InvalidOperationException("Could not locate mod root containing wardrobe-items.json");

        return Path.Combine(dir.FullName, "AgeRegression");
    }

    // ---------------------------------------------------------------------
    // Real migrated production file
    // ---------------------------------------------------------------------

    [Fact]
    public void ProductionWardrobeItems_PennyGift_FriendshipUnlockMigrated()
    {
        var modRoot = FindModRoot();
        var loader  = new DataLoader(
            new FileSystemAssetProvider(modRoot, new LogHelper(NullMonitor.Instance)),
            new LogHelper(NullMonitor.Instance));
        loader.LoadAll();

        var gift = loader.GetWardrobeItem("penny_gift");
        gift.Should().NotBeNull();
        var item = ResolvedWardrobeItem.FromAccessory(gift!);

        new ItemUnlockService(new StubGameState(friendship: new() { ["Penny"] = 1000 }))
            .IsUnlocked(item).Should().BeTrue();
        new ItemUnlockService(new StubGameState(friendship: new() { ["Penny"] = 500 }))
            .IsUnlocked(item).Should().BeFalse();

        // Unrelated item without an unlock requirement stays available.
        var pacifier = loader.GetWardrobeItem("pacifier");
        new ItemUnlockService(new StubGameState())
            .IsUnlocked(ResolvedWardrobeItem.FromAccessory(pacifier!))
            .Should().BeTrue();
    }

    // ---------------------------------------------------------------------
    // New Conditions format loads and evaluates correctly via DataLoader
    // ---------------------------------------------------------------------

    [Fact]
    public void MigratedYearCondition_EvaluatesThroughPipeline()
    {
        const string diapers = """
            [
              { "Id": "mig_item", "Price": 80, "ShopAvailable": true,
                "Unlock": { "Conditions": [ { "Type": "Year", "Value": 2 } ] } }
            ]
            """;

        var item = LoadDiaper(diapers, "mig_item");

        new ItemUnlockService(new StubGameState(year: 1)).IsUnlocked(item).Should().BeFalse();
        new ItemUnlockService(new StubGameState(year: 2)).IsUnlocked(item).Should().BeTrue();
        new ItemUnlockService(new StubGameState(year: 3)).IsUnlocked(item).Should().BeTrue();
    }

    [Fact]
    public void MigratedSeasonCondition_EvaluatesThroughPipeline()
    {
        const string diapers = """
            [
              { "Id": "mig_item", "Price": 80, "ShopAvailable": true,
                "Unlock": { "Conditions": [ { "Type": "Season", "Value": "summer" } ] } }
            ]
            """;

        var item = LoadDiaper(diapers, "mig_item");

        new ItemUnlockService(new StubGameState(season: "spring")).IsUnlocked(item).Should().BeFalse();
        new ItemUnlockService(new StubGameState(season: "summer")).IsUnlocked(item).Should().BeTrue();
        new ItemUnlockService(new StubGameState(season: "fall")).IsUnlocked(item).Should().BeFalse();
    }

    [Fact]
    public void MigratedFriendshipCondition_EvaluatesThroughPipeline()
    {
        const string diapers = """
            [
              { "Id": "mig_item", "Price": 80, "ShopAvailable": true,
                "Unlock": { "Conditions": [ { "Type": "Friendship", "Npc": "Penny", "Points": 1000 } ] } }
            ]
            """;

        var item = LoadDiaper(diapers, "mig_item");

        new ItemUnlockService(new StubGameState(friendship: new() { ["Penny"] = 500 }))
            .IsUnlocked(item).Should().BeFalse();
        new ItemUnlockService(new StubGameState(friendship: new() { ["Penny"] = 1000 }))
            .IsUnlocked(item).Should().BeTrue();
    }

    [Fact]
    public void MigratedMailFlagCondition_EvaluatesThroughPipeline()
    {
        const string diapers = """
            [
              { "Id": "mig_item", "Price": 80, "ShopAvailable": true,
                "Unlock": { "Conditions": [ { "Type": "MailFlag", "Flag": "example" } ] } }
            ]
            """;

        var item = LoadDiaper(diapers, "mig_item");

        new ItemUnlockService(new StubGameState(mailFlags: new HashSet<string>()))
            .IsUnlocked(item).Should().BeFalse();
        new ItemUnlockService(new StubGameState(mailFlags: new HashSet<string> { "example" }))
            .IsUnlocked(item).Should().BeTrue();
    }

    [Fact]
    public void MigratedMultipleConditions_EvaluatesThroughPipeline()
    {
        const string diapers = """
            [
              {
                "Id": "mig_item", "Price": 80, "ShopAvailable": true,
                "Unlock": {
                  "Conditions": [
                    { "Type": "Year", "Value": 2 },
                    { "Type": "Season", "Value": "summer" },
                    { "Type": "MailFlag", "Flag": "example" }
                  ]
                }
              }
            ]
            """;

        var item = LoadDiaper(diapers, "mig_item");

        foreach (var year in new[] { 1, 2, 3 })
        foreach (var season in new[] { "spring", "summer" })
        foreach (var hasMail in new[] { false, true })
        {
            var mail = hasMail ? new HashSet<string> { "example" } : new HashSet<string>();
            var service = new ItemUnlockService(new StubGameState(year: year, season: season, mailFlags: mail));
            var expected = year >= 2 && season == "summer" && hasMail;
            service.IsUnlocked(item).Should().Be(
                expected,
                "multi-condition unlock must match AND of (year>=2, summer, mail) at (year {0}, {1}, mail {2})",
                year, season, hasMail);
        }
    }
}
