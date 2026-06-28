using AgeRegression.Data;
using AgeRegression.Utilities;
using FluentAssertions;

namespace AgeRegression.Tests;

public sealed class DataLoaderFallbackTests
{
    [Fact]
    public void LoadAll_WhenStagesFileMissing_UsesBuiltInDefaults()
    {
        var loader = new DataLoader(new EmptyAssetProvider(), new LogHelper(NullMonitor.Instance));
        loader.LoadAll();
        loader.Stages.Should().NotBeEmpty();
        loader.Stages.Should().Contain(s => s.Id == "none");
        loader.Stages.Should().Contain(s => s.Id == "baby");
    }

    [Fact]
    public void LoadAll_WhenDiaperFileMissing_ProducesEmptyList()
    {
        var loader = new DataLoader(new EmptyAssetProvider(), new LogHelper(NullMonitor.Instance));
        loader.LoadAll();
        loader.DiaperTypes.Should().BeEmpty();
    }

    [Fact]
    public void LoadAll_StagesAreSortedByOrder()
    {
        var loader = new DataLoader(new EmptyAssetProvider(), new LogHelper(NullMonitor.Instance));
        loader.LoadAll();
        loader.Stages.Select(s => s.Order).Should().BeInAscendingOrder();
    }

    [Fact]
    public void GetBaselineStage_ReturnsLowestOrderStage()
    {
        var loader = new DataLoader(new EmptyAssetProvider(), new LogHelper(NullMonitor.Instance));
        loader.LoadAll();
        var baseline = loader.GetBaselineStage();
        baseline.Should().NotBeNull();
        baseline!.Order.Should().Be(loader.Stages.Min(s => s.Order));
    }

    [Fact]
    public void LoadAll_DeduplicatesDuplicateStageIds()
    {
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(new[]
        {
            new { Id = "none",   DisplayName = "Normal",    Order = 0,
                  Description = "", StatModifiers = new { } },
            new { Id = "none",   DisplayName = "Duplicate", Order = 1,
                  Description = "", StatModifiers = new { } },
            new { Id = "little", DisplayName = "Little",    Order = 2,
                  Description = "", StatModifiers = new { } }
        });

        var provider = new InMemoryAssetProvider(
            new Dictionary<string, string>
            {
                ["assets/data/regression-stages.json"] = json
            });

        var loader = new DataLoader(provider, new LogHelper(NullMonitor.Instance));
        loader.LoadAll();
        loader.Stages.Count(s => s.Id == "none").Should().Be(1);
        loader.Stages.Should().HaveCount(2);
    }

    [Fact]
    public void DefaultStage_None_HasNoStatPenalties()
    {
        var loader = new DataLoader(new EmptyAssetProvider(), new LogHelper(NullMonitor.Instance));
        loader.LoadAll();
        var none = loader.GetStage("none");
        none.Should().NotBeNull();
        none!.StatModifiers.SpeedMultiplier.Should().Be(1.0f);
        none.StatModifiers.CanUseTools.Should().BeTrue();
    }

    [Fact]
    public void DefaultStage_Baby_HasSignificantPenalties()
    {
        var loader = new DataLoader(new EmptyAssetProvider(), new LogHelper(NullMonitor.Instance));
        loader.LoadAll();
        var baby = loader.GetStage("baby");
        baby.Should().NotBeNull();
        baby!.StatModifiers.SpeedMultiplier.Should().BeLessThan(1.0f);
        baby.StatModifiers.CanUseTools.Should().BeFalse();
    }
}
