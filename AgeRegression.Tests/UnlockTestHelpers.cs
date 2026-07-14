using System.Collections.Generic;
using AgeRegression.Data;
using AgeRegression.Items;
using AgeRegression.Tests;
using AgeRegression.Utilities;
using FluentAssertions;

namespace AgeRegression.Tests;

/// <summary>
/// Shared helpers for the unlock-condition tests: builds an in-memory
/// <see cref="DataLoader"/> and resolves a single diaper definition from it.
/// </summary>
internal static class UnlockTestHelpers
{
    internal static DataLoader BuildLoader(string diaperJson = "[]", string wardrobeJson = "[]")
    {
        var files = new Dictionary<string, string>
        {
            ["assets/data/diaper-types.json"]  = diaperJson,
            ["assets/data/wardrobe-items.json"] = wardrobeJson
        };
        var loader = new DataLoader(
            new InMemoryAssetProvider(files),
            new LogHelper(NullMonitor.Instance));
        loader.LoadAll();
        return loader;
    }

    internal static ResolvedWardrobeItem LoadDiaper(string diaperJson, string itemId)
    {
        var def = BuildLoader(diaperJson: diaperJson).GetDiaperType(itemId);
        def.Should().NotBeNull();
        return ResolvedWardrobeItem.FromDiaper(def!);
    }
}
