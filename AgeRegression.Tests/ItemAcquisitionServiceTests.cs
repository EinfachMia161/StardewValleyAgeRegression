using AgeRegression.Data;
using AgeRegression.Items;
using AgeRegression.Utilities;
using FluentAssertions;

// The test stub lives in the same test assembly namespace and is used here.

namespace AgeRegression.Tests;

public sealed class ItemAcquisitionServiceTests
{
    private static DataLoader BuildLoader(string diaperJson = "[]", string wardrobeJson = "[]")
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

    private static ItemAcquisitionService BuildService(
        DataLoader? loader = null,
        IGameStateProvider? gameState = null,
        IItemFactory? itemFactory = null)
    {
        loader ??= BuildLoader();
        gameState ??= new StubGameState();
        itemFactory ??= new StubItemFactory();

        var resolver = new WardrobeItemResolver(loader);
        var unlockService = new ItemUnlockService(gameState);
        return new ItemAcquisitionService(resolver, unlockService, itemFactory);
    }

    [Fact]
    public void Acquire_UnknownItem_ReturnsUnknownItemFailure()
    {
        var service = BuildService();

        var result = service.Acquire("missing_item", new AcquisitionContext(Source: AcquisitionSource.Shop));

        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(AcquisitionFailureReason.UnknownItem);
    }

    [Fact]
    public void Acquire_LockedItem_ReturnsLockedFailure()
    {
        var loader = BuildLoader(
            diaperJson: "[{ \"Id\": \"locked_diaper\", \"Price\": 50, \"Unlock\": { \"Conditions\": [ { \"Type\": \"Year\", \"Value\": 2 } ] } }]"
        );
        var service = BuildService(loader, new StubGameState(year: 1));

        var result = service.Acquire("locked_diaper", new AcquisitionContext(Source: AcquisitionSource.Shop));

        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(AcquisitionFailureReason.Locked);
    }

    [Fact]
    public void Acquire_UnlockedItem_ReturnsCreatedItem()
    {
        var loader = BuildLoader(
            diaperJson: "[{ \"Id\": \"available_diaper\", \"Price\": 50 }]"
        );
        var itemFactory = new StubItemFactory();
        var service = BuildService(loader, new StubGameState(), itemFactory);

        var result = service.Acquire("available_diaper", new AcquisitionContext(Source: AcquisitionSource.Shop));

        result.Success.Should().BeTrue();
        result.FailureReason.Should().Be(AcquisitionFailureReason.None);
        result.ResolvedItem.Should().NotBeNull();
    }

    [Fact]
    public void Acquire_CreationFailure_ReturnsCreationFailed()
    {
        var loader = BuildLoader(
            diaperJson: "[{ \"Id\": \"bad_diaper\", \"Price\": 50 }]"
        );
        var itemFactory = new StubItemFactory();
        var service = BuildService(loader, new StubGameState(), itemFactory);

        var result = service.Acquire("bad_diaper", new AcquisitionContext(Source: AcquisitionSource.Shop));

        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(AcquisitionFailureReason.CreationFailed);
    }
}
