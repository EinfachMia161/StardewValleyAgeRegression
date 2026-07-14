using AgeRegression.Data;
using AgeRegression.Dialogue;
using AgeRegression.Utilities;
using FluentAssertions;

namespace AgeRegression.Tests;

public sealed class DiaperTypeTests
{
private const string DiaperTypesJson = @"[
  {
    ""Id"": ""basic_diaper"",
    ""DisplayName"": ""Basic Diaper"",
    ""SpriteSheet"": ""assets/sprites/diapers.png"",
    ""SpriteIndex"": 0,
    ""Rarity"": ""common""
  },
  {
    ""Id"": ""soft_diaper"",
    ""DisplayName"": ""Soft Comfort Diaper"",
    ""SpriteSheet"": ""assets/sprites/diapers.png"",
    ""SpriteIndex"": 1,
    ""Rarity"": ""common""
  },
  {
    ""Id"": ""premium_diaper"",
    ""DisplayName"": ""Premium Diaper"",
    ""SpriteSheet"": ""assets/sprites/diapers.png"",
    ""SpriteIndex"": 2,
    ""Rarity"": ""rare""
  },
  {
    ""Id"": ""overnight_diaper"",
    ""DisplayName"": ""Overnight Diaper"",
    ""SpriteSheet"": ""assets/sprites/diapers.png"",
    ""SpriteIndex"": 3,
    ""Rarity"": ""uncommon""
  },
  {
    ""Id"": ""training_diaper"",
    ""DisplayName"": ""Training Diaper"",
    ""SpriteSheet"": ""assets/sprites/diapers.png"",
    ""SpriteIndex"": 4,
    ""Rarity"": ""common""
  },
  {
    ""Id"": ""cute_diaper"",
    ""DisplayName"": ""Cute Pattern Diaper"",
    ""SpriteSheet"": ""assets/sprites/diapers.png"",
    ""SpriteIndex"": 5,
    ""Rarity"": ""uncommon""
  },
  {
    ""Id"": ""medical_diaper"",
    ""DisplayName"": ""Medical Diaper"",
    ""SpriteSheet"": ""assets/sprites/diapers.png"",
    ""SpriteIndex"": 6,
    ""Rarity"": ""rare""
  },
  {
    ""Id"": ""luxury_diaper"",
    ""DisplayName"": ""Luxury Diaper"",
    ""SpriteSheet"": ""assets/sprites/diapers.png"",
    ""SpriteIndex"": 7,
    ""Rarity"": ""legendary""
  }
]";

    private const string ComfortModifiersJson = @"[
  {
    ""Id"": ""premium_diaper_equipped"",
    ""Description"": ""Wearing a premium diaper"",
    ""ValuePerHour"": 2.0,
    ""ImmediateValue"": 5.0,
    ""Conditions"": {
      ""EquippedDiaperIds"": [ ""premium_diaper"" ]
    },
    ""Priority"": 7
  },
  {
    ""Id"": ""luxury_diaper_equipped"",
    ""Description"": ""Wearing a luxury diaper"",
    ""ValuePerHour"": 5.0,
    ""ImmediateValue"": 10.0,
    ""Conditions"": {
      ""EquippedDiaperIds"": [ ""luxury_diaper"" ]
    },
    ""Priority"": 7
  },
  {
    ""Id"": ""medical_diaper_equipped"",
    ""Description"": ""Wearing a medical diaper"",
    ""ValuePerHour"": 3.0,
    ""ImmediateValue"": 4.0,
    ""Conditions"": {
      ""EquippedDiaperIds"": [ ""medical_diaper"" ]
    },
    ""Priority"": 7
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
    }

    [Fact]
    public void Diapers_HaveSpriteInformation()
    {
        var provider = new InMemoryAssetProvider(
            new Dictionary<string, string>
            {
                ["assets/data/diaper-types.json"] = DiaperTypesJson
            });

        var loader = new DataLoader(provider, new LogHelper(NullMonitor.Instance));
        loader.LoadAll();

        loader.DiaperTypes.Should().NotBeEmpty();
        loader.DiaperTypes.All(d => !string.IsNullOrEmpty(d.SpriteSheet)).Should().BeTrue();
    }

    [Fact]
    public void DiaperTypes_SpriteIndicesAreUnique()
    {
        var provider = new InMemoryAssetProvider(
            new Dictionary<string, string>
            {
                ["assets/data/diaper-types.json"] = DiaperTypesJson
            });

        var loader = new DataLoader(provider, new LogHelper(NullMonitor.Instance));
        loader.LoadAll();

        var indices = loader.DiaperTypes.Select(d => d.SpriteIndex).ToList();
        indices.Distinct().Should().HaveSameCount(indices);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 64, 0)]
    [InlineData(13, 0, 64)]
    [InlineData(26, 0, 128)]
    [InlineData(7, 448, 0)]
    public void SpriteIndex_CalculatesCorrectPosition(int spriteIndex, int expectedX, int expectedY)
    {
        var column = spriteIndex % 13;
        var row = spriteIndex / 13;
        var x = column * 64;
        var y = row * 64;

        x.Should().Be(expectedX);
        y.Should().Be(expectedY);
    }

    // --- Comfort Modifier Tests ---

    [Fact]
    public void ComfortModifiers_JsonLoadsSuccessfully()
    {
        var provider = new InMemoryAssetProvider(
            new Dictionary<string, string>
            {
                ["assets/data/comfort-modifiers.json"] = ComfortModifiersJson
            });

        var loader = new DataLoader(provider, new LogHelper(NullMonitor.Instance));
        loader.LoadAll();

        loader.ComfortModifiers.Should().HaveCount(3);
        loader.ComfortModifiers.Should().Contain(m => m.Id == "premium_diaper_equipped");
        loader.ComfortModifiers.Should().Contain(m => m.Id == "luxury_diaper_equipped");
        loader.ComfortModifiers.Should().Contain(m => m.Id == "medical_diaper_equipped");
    }

    [Fact]
    public void EquippedDiaperIdsCondition_MatchesPremiumDiaper()
    {
        var evaluator = new DialogueConditionEvaluator();
        var modifier = new ComfortModifierData
        {
            Id = "premium_diaper_equipped",
            Conditions = new DialogueConditions
            {
                EquippedDiaperIds = new List<string> { "premium_diaper" }
            }
        };

        var ctx = MakeTestContext(equippedDiaperTypeId: "premium_diaper");
        evaluator.Evaluate(modifier.Conditions, ctx).Should().BeTrue();
    }

    [Fact]
    public void EquippedDiaperIdsCondition_DoesNotMatchDifferentDiaper()
    {
        var evaluator = new DialogueConditionEvaluator();
        var modifier = new ComfortModifierData
        {
            Id = "premium_diaper_equipped",
            Conditions = new DialogueConditions
            {
                EquippedDiaperIds = new List<string> { "premium_diaper" }
            }
        };

        var ctx = MakeTestContext(equippedDiaperTypeId: "basic_diaper");
        evaluator.Evaluate(modifier.Conditions, ctx).Should().BeFalse();
    }

    [Fact]
    public void PremiumDiaper_ModifierAppliesWhenEquipped()
    {
        var provider = new InMemoryAssetProvider(
            new Dictionary<string, string>
            {
                ["assets/data/comfort-modifiers.json"] = ComfortModifiersJson
            });

        var loader = new DataLoader(provider, new LogHelper(NullMonitor.Instance));
        loader.LoadAll();

        var premiumModifier = loader.ComfortModifiers.FirstOrDefault(m => m.Id == "premium_diaper_equipped");
        premiumModifier.Should().NotBeNull();
        premiumModifier!.ValuePerHour.Should().Be(2.0f);
        premiumModifier.ImmediateValue.Should().Be(5.0f);
    }

    [Fact]
    public void LuxuryDiaper_ModifierHasHigherValuesThanPremium()
    {
        var provider = new InMemoryAssetProvider(
            new Dictionary<string, string>
            {
                ["assets/data/comfort-modifiers.json"] = ComfortModifiersJson
            });

        var loader = new DataLoader(provider, new LogHelper(NullMonitor.Instance));
        loader.LoadAll();

        var premiumModifier = loader.ComfortModifiers.First(m => m.Id == "premium_diaper_equipped");
        var luxuryModifier = loader.ComfortModifiers.First(m => m.Id == "luxury_diaper_equipped");

        luxuryModifier.ImmediateValue.Should().BeGreaterThan(premiumModifier.ImmediateValue);
        luxuryModifier.ValuePerHour.Should().BeGreaterThan(premiumModifier.ValuePerHour);
    }

    [Fact]
    public void SpriteAsset_PathIsValidForStandardFormat()
    {
        var provider = new InMemoryAssetProvider(
            new Dictionary<string, string>
            {
                ["assets/data/diaper-types.json"] = DiaperTypesJson
            });

        var loader = new DataLoader(provider, new LogHelper(NullMonitor.Instance));
        loader.LoadAll();

        loader.DiaperTypes.All(d => d.SpriteSheet == "assets/sprites/diapers.png").Should().BeTrue();
    }

    private static DialogueContext MakeTestContext(string equippedDiaperTypeId = "")
    {
        return new DialogueContext
        {
            RegressionStageId = "none",
            FriendshipHearts = 0,
            IsMarried = false,
            Season = "spring",
            TimeOfDay = 1000,
            Weather = "sunny",
            LocationName = "Farm",
            IsWearingDiaper = !string.IsNullOrEmpty(equippedDiaperTypeId),
            EquippedDiaperTypeId = equippedDiaperTypeId,
            DiaperConditionId = "clean",
            ContinenceNormalized = 1f,
            ContinenceThresholdId = "comfortable",
            HungerNormalized = 1f,
            ThirstNormalized = 1f,
            ComfortNormalized = 1f,
            NpcPersonalityTags = Array.Empty<string>(),
            EquippedAccessories = new HashSet<string>(),
            GameFlags = new HashSet<string>(),
            LastCareActionId = string.Empty,
            CareActionsToday = 0,
            DaysSinceLastDiaperChange = 0
        };
    }
}