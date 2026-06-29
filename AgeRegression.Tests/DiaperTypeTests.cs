using AgeRegression.Data;
using AgeRegression.Utilities;
using FluentAssertions;

namespace AgeRegression.Tests;

public sealed class DiaperTypeTests
{
    private const string DiaperTypesJson = @"[
  {
    ""Id"": ""basic_diaper"",
    ""DisplayName"": ""Basic Diaper"",
    ""Description"": ""A simple, comfortable diaper. Reliable and affordable."",
    ""MaxCapacity"": 100.0,
    ""AbsorptionRate"": 1.0,
    ""Thickness"": 2,
    ""Padding"": 2,
    ""ThicknessSpeedPenalty"": 0.98,
    ""SupportsBooster"": false,
    ""EquipComfortBonus"": 5.0,
    ""ChangeComfortBonusRatio"": 0.8,
    ""RemoveComfortDelta"": -3.0,
    ""CleanComfortPerHour"": 3.0,
    ""SaturatedComfortPenalty"": 20.0,
    ""ContinenceDrainMultiplier"": 0.9,
    ""SpriteIndex"": 0,
    ""Price"": 50,
    ""Rarity"": ""common""
  },
  {
    ""Id"": ""soft_diaper"",
    ""DisplayName"": ""Soft Comfort Diaper"",
    ""Description"": ""Higher comfort, lower capacity. Gentle on skin."",
    ""MaxCapacity"": 80.0,
    ""AbsorptionRate"": 0.9,
    ""Thickness"": 1,
    ""Padding"": 3,
    ""ThicknessSpeedPenalty"": 1.0,
    ""SupportsBooster"": false,
    ""EquipComfortBonus"": 7.0,
    ""ChangeComfortBonusRatio"": 0.7,
    ""RemoveComfortDelta"": -2.0,
    ""CleanComfortPerHour"": 4.0,
    ""SaturatedComfortPenalty"": 25.0,
    ""ContinenceDrainMultiplier"": 0.95,
    ""SpriteIndex"": 3,
    ""Price"": 80,
    ""Rarity"": ""common""
  },
  {
    ""Id"": ""premium_diaper"",
    ""DisplayName"": ""Premium Diaper"",
    ""Description"": ""A thick, high-capacity diaper with extra padding. Very secure."",
    ""MaxCapacity"": 175.0,
    ""AbsorptionRate"": 1.2,
    ""Thickness"": 4,
    ""Padding"": 4,
    ""ThicknessSpeedPenalty"": 0.94,
    ""SupportsBooster"": true,
    ""EquipComfortBonus"": 10.0,
    ""ChangeComfortBonusRatio"": 0.85,
    ""RemoveComfortDelta"": -5.0,
    ""CleanComfortPerHour"": 5.0,
    ""SaturatedComfortPenalty"": 15.0,
    ""ContinenceDrainMultiplier"": 0.8,
    ""SpriteIndex"": 1,
    ""Price"": 120,
    ""Rarity"": ""rare""
  },
  {
    ""Id"": ""overnight_diaper"",
    ""DisplayName"": ""Overnight Diaper"",
    ""Description"": ""High capacity for long nights."",
    ""MaxCapacity"": 200.0,
    ""AbsorptionRate"": 1.3,
    ""Thickness"": 4,
    ""Padding"": 3,
    ""ThicknessSpeedPenalty"": 0.92,
    ""SupportsBooster"": true,
    ""EquipComfortBonus"": 6.0,
    ""ChangeComfortBonusRatio"": 0.8,
    ""RemoveComfortDelta"": -4.0,
    ""CleanComfortPerHour"": 4.0,
    ""SaturatedComfortPenalty"": 10.0,
    ""ContinenceDrainMultiplier"": 0.85,
    ""SpriteIndex"": 4,
    ""Price"": 150,
    ""Rarity"": ""uncommon""
  },
  {
    ""Id"": ""training_diaper"",
    ""DisplayName"": ""Training Diaper"",
    ""Description"": ""A lighter diaper for those just starting out. Less bulk, less security."",
    ""MaxCapacity"": 60.0,
    ""AbsorptionRate"": 0.8,
    ""Thickness"": 1,
    ""Padding"": 1,
    ""ThicknessSpeedPenalty"": 1.0,
    ""SupportsBooster"": false,
    ""EquipComfortBonus"": 2.0,
    ""ChangeComfortBonusRatio"": 0.75,
    ""RemoveComfortDelta"": -1.0,
    ""CleanComfortPerHour"": 1.5,
    ""SaturatedComfortPenalty"": 25.0,
    ""ContinenceDrainMultiplier"": 0.95,
    ""SpriteIndex"": 2,
    ""Price"": 30,
    ""Rarity"": ""common""
  },
  {
    ""Id"": ""cute_diaper"",
    ""DisplayName"": ""Cute Pattern Diaper"",
    ""Description"": ""Pretty patterns make this diaper extra comforting."",
    ""MaxCapacity"": 90.0,
    ""AbsorptionRate"": 1.0,
    ""Thickness"": 2,
    ""Padding"": 3,
    ""ThicknessSpeedPenalty"": 0.97,
    ""SupportsBooster"": false,
    ""EquipComfortBonus"": 12.0,
    ""ChangeComfortBonusRatio"": 0.9,
    ""RemoveComfortDelta"": -4.0,
    ""CleanComfortPerHour"": 6.0,
    ""SaturatedComfortPenalty"": 18.0,
    ""ContinenceDrainMultiplier"": 0.9,
    ""SpriteIndex"": 5,
    ""Price"": 100,
    ""Rarity"": ""uncommon""
  },
  {
    ""Id"": ""medical_diaper"",
    ""DisplayName"": ""Medical Diaper"",
    ""Description"": ""Maximum protection with clinical absorbency."",
    ""MaxCapacity"": 250.0,
    ""AbsorptionRate"": 1.5,
    ""Thickness"": 5,
    ""Padding"": 4,
    ""ThicknessSpeedPenalty"": 0.9,
    ""SupportsBooster"": true,
    ""EquipComfortBonus"": 4.0,
    ""ChangeComfortBonusRatio"": 0.7,
    ""RemoveComfortDelta"": -6.0,
    ""CleanComfortPerHour"": 2.0,
    ""SaturatedComfortPenalty"": 5.0,
    ""ContinenceDrainMultiplier"": 0.75,
    ""SpriteIndex"": 6,
    ""Price"": 200,
    ""Rarity"": ""rare""
  },
  {
    ""Id"": ""luxury_diaper"",
    ""DisplayName"": ""Luxury Diaper"",
    ""Description"": ""Top-tier comfort and quality for discerning littles."",
    ""MaxCapacity"": 150.0,
    ""AbsorptionRate"": 1.1,
    ""Thickness"": 3,
    ""Padding"": 5,
    ""ThicknessSpeedPenalty"": 0.95,
    ""SupportsBooster"": true,
    ""EquipComfortBonus"": 15.0,
    ""ChangeComfortBonusRatio"": 0.95,
    ""RemoveComfortDelta"": -8.0,
    ""CleanComfortPerHour"": 8.0,
    ""SaturatedComfortPenalty"": 12.0,
    ""ContinenceDrainMultiplier"": 0.85,
    ""SpriteIndex"": 7,
    ""Price"": 500,
    ""Rarity"": ""legendary""
  }
]";

    [Fact]
    public void DiaperTypes_JsonContainsAll8Types()
    {
        var provider = new InMemoryAssetProvider(
            new Dictionary<string, string>
            {
                ["assets/data/diaper-types.json"] = DiaperTypesJson
            });

        var loader = new DataLoader(provider, new LogHelper(NullMonitor.Instance));
        loader.LoadAll();

        // Check all 8 diaper types exist
        var expectedIds = new[]
        {
            "basic_diaper", "soft_diaper", "premium_diaper", "overnight_diaper",
            "training_diaper", "cute_diaper", "medical_diaper", "luxury_diaper"
        };

        loader.DiaperTypes.Should().HaveCount(8);
        foreach (var id in expectedIds)
        {
            loader.GetDiaperType(id).Should().NotBeNull($"Diaper type '{id}' should exist");
        }
    }

    [Fact]
    public void DiaperTypes_AllIdsAreUnique()
    {
        var provider = new InMemoryAssetProvider(
            new Dictionary<string, string>
            {
                ["assets/data/diaper-types.json"] = DiaperTypesJson
            });

        var loader = new DataLoader(provider, new LogHelper(NullMonitor.Instance));
        loader.LoadAll();

        var ids = loader.DiaperTypes.Select(d => d.Id).ToList();
        ids.Distinct().Should().HaveSameCount(ids);
    }

    [Fact]
    public void DiaperTypes_PremiumHasCorrectProperties()
    {
        var provider = new InMemoryAssetProvider(
            new Dictionary<string, string>
            {
                ["assets/data/diaper-types.json"] = DiaperTypesJson
            });

        var loader = new DataLoader(provider, new LogHelper(NullMonitor.Instance));
        loader.LoadAll();

        var premium = loader.GetDiaperType("premium_diaper");
        premium.Should().NotBeNull();
        premium!.DisplayName.Should().Be("Premium Diaper");
        premium.Rarity.Should().Be("rare");
        premium.Price.Should().Be(120);
    }

    [Fact]
    public void Diapers_HaveRarityProperty()
    {
        var provider = new InMemoryAssetProvider(
            new Dictionary<string, string>
            {
                ["assets/data/diaper-types.json"] = DiaperTypesJson
            });

        var loader = new DataLoader(provider, new LogHelper(NullMonitor.Instance));
        loader.LoadAll();

        loader.DiaperTypes.Should().NotBeEmpty();
        loader.DiaperTypes.All(d => !string.IsNullOrEmpty(d.Rarity)).Should().BeTrue();
    }
}