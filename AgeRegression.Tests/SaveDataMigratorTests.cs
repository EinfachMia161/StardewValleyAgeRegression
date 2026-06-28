using AgeRegression.Persistence;
using AgeRegression.Utilities;
using FluentAssertions;

namespace AgeRegression.Tests;

public sealed class SaveDataMigratorTests
{
    private readonly SaveDataMigrator _migrator = new(new LogHelper(NullMonitor.Instance));

    [Fact]
    public void Migrate_WhenSchemaIsCurrentVersion_ReturnsFalse()
    {
        var data = new SaveDataModel
        {
            SchemaVersion = SaveDataMigrator.CurrentSchemaVersion
        };
        _migrator.Migrate(data).Should().BeFalse();
        data.SchemaVersion.Should().Be(SaveDataMigrator.CurrentSchemaVersion);
    }

    [Fact]
    public void Migrate_WhenSchemaIsCurrentVersion_DoesNotModifyData()
    {
        var data = new SaveDataModel
        {
            SchemaVersion  = SaveDataMigrator.CurrentSchemaVersion,
            CurrentStageId = "little",
            ComfortScore   = 75f
        };
        _migrator.Migrate(data);
        data.CurrentStageId.Should().Be("little");
        data.ComfortScore.Should().Be(75f);
    }

    [Fact]
    public void Migrate_WhenSchemaIsNewerThanCurrent_ReturnsFalse()
    {
        var data = new SaveDataModel
        {
            SchemaVersion = SaveDataMigrator.CurrentSchemaVersion + 1
        };
        _migrator.Migrate(data).Should().BeFalse();
    }

    [Fact]
    public void Migrate_WhenSchemaIsNewerThanCurrent_DoesNotChangeVersion()
    {
        var futureVersion = SaveDataMigrator.CurrentSchemaVersion + 5;
        var data = new SaveDataModel { SchemaVersion = futureVersion };
        _migrator.Migrate(data);
        data.SchemaVersion.Should().Be(futureVersion);
    }

    [Fact]
    public void Migration_V1ToV2_SetsSchemaVersion2()
    {
        var data = new SaveDataModel
        {
            SchemaVersion  = 1,
            CurrentStageId = "little"
        };
        var migrated = _migrator.Migrate(data);
        migrated.Should().BeTrue();
        data.SchemaVersion.Should().Be(2);
    }

    [Fact]
    public void Migration_V1ToV2_PreservesExistingFields()
    {
        var data = new SaveDataModel
        {
            SchemaVersion  = 1,
            CurrentStageId = "baby",
            ComfortScore   = 42f,
            DiaperWetness  = 0.5f
        };
        _migrator.Migrate(data);
        data.CurrentStageId.Should().Be("baby");
        data.ComfortScore.Should().Be(42f);
        data.DiaperWetness.Should().Be(0.5f);
    }

    [Fact]
    public void Migration_V1ToV2_NeedsFieldsHaveSafeDefaults()
    {
        var data = new SaveDataModel { SchemaVersion = 1 };
        _migrator.Migrate(data);
        data.ContinenceNormalized.Should().Be(1.0f);
        data.HungerNormalized.Should().Be(1.0f);
        data.ThirstNormalized.Should().Be(1.0f);
        data.ContinenceStressModifier.Should().Be(0f);
    }

    [Fact]
    public void FreshSaveDataModel_HasExpectedDefaults()
    {
        var data = new SaveDataModel();
        data.SchemaVersion.Should().Be(SaveDataMigrator.CurrentSchemaVersion);
        data.CurrentStageId.Should().Be("none");
        data.EquippedDiaperTypeId.Should().BeNull();
        data.DiaperWetness.Should().Be(0f);
        data.ComfortScore.Should().Be(50f);
        data.EquippedAccessories.Should().BeEmpty();
        data.DialogueCooldowns.Should().BeEmpty();
    }

    [Fact]
    public void SaveDataModel_RoundTripsViaJson()
    {
        var original = new SaveDataModel
        {
            SchemaVersion          = 2,
            CurrentStageId         = "baby",
            EquippedDiaperTypeId   = "premium",
            DiaperWetness          = 0.45f,
            DiaperMessing          = 0.1f,
            DiaperHasBooster       = true,
            DiaperLastChangedAbsoluteDay = 42,
            ComfortScore           = 33.5f,
            EquippedAccessories    = new List<string> { "pacifier", "mittens" },
            DialogueCooldowns      = new Dictionary<string, int>
            {
                ["Abigail:greet_little"] = 10
            },
            LastSavedAbsoluteDay   = 55,
            ContinenceNormalized   = 0.6f,
            HungerNormalized       = 0.8f,
            ThirstNormalized       = 0.7f
        };

        var json     = Newtonsoft.Json.JsonConvert.SerializeObject(original);
        var restored = Newtonsoft.Json.JsonConvert
            .DeserializeObject<SaveDataModel>(json);

        restored.Should().NotBeNull();
        restored!.SchemaVersion.Should().Be(original.SchemaVersion);
        restored.CurrentStageId.Should().Be(original.CurrentStageId);
        restored.EquippedDiaperTypeId.Should().Be(original.EquippedDiaperTypeId);
        restored.DiaperWetness.Should().BeApproximately(
            original.DiaperWetness, 0.0001f);
        restored.DiaperHasBooster.Should().Be(original.DiaperHasBooster);
        restored.ComfortScore.Should().BeApproximately(
            original.ComfortScore, 0.0001f);
        restored.EquippedAccessories.Should()
            .BeEquivalentTo(original.EquippedAccessories);
        restored.ContinenceNormalized.Should().BeApproximately(
            original.ContinenceNormalized, 0.0001f);
    }
}
