using AgeRegression.State;
using AgeRegression.Utilities;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;

namespace AgeRegression.Persistence;

/// <summary>
/// Reads and writes <see cref="SaveDataModel"/> to and from
/// <c>farmer.modData</c> as a single JSON-encoded entry.
///
/// <para>
/// Using a single JSON blob rather than individual modData keys keeps
/// the save file clean and makes schema migration straightforward.
/// </para>
/// </summary>
public sealed class PersistenceManager
{
    private const string SaveDataKey = "mia.AgeRegression/SaveData";

    private readonly IModHelper _helper;
    private readonly LogHelper _log;
    private readonly SaveDataMigrator _migrator;

    public PersistenceManager(IModHelper helper, LogHelper log)
    {
        _helper  = helper;
        _log     = log;
        _migrator = new SaveDataMigrator(log);
    }

    // -------------------------------------------------------------------------
    // Load
    // -------------------------------------------------------------------------

    /// <summary>
    /// Loads save data for the given farmer, running migrations if needed.
    /// Returns a fresh default model if no data exists.
    /// </summary>
    public SaveDataModel Load(Farmer farmer)
    {
        if (!farmer.modData.TryGetValue(SaveDataKey, out var json) ||
            string.IsNullOrWhiteSpace(json))
        {
            _log.Debug("No existing save data for '{0}'. Creating defaults.",
                farmer.Name);
            return new SaveDataModel();
        }

        SaveDataModel? data;
        try
        {
            data = JsonConvert.DeserializeObject<SaveDataModel>(json);
        }
        catch (Exception ex)
        {
            _log.Exception(
                $"Failed to deserialize save data for '{farmer.Name}'. " +
                "Using defaults.", ex);
            return new SaveDataModel();
        }

        if (data is null)
        {
            _log.Warn(
                "Deserialized save data was null for '{0}'. Using defaults.",
                farmer.Name);
            return new SaveDataModel();
        }

        if (_migrator.Migrate(data))
        {
            _log.Debug(
                "Save data migrated for '{0}'. Persisting immediately.",
                farmer.Name);
            Save(farmer, data);
        }

        return data;
    }

    // -------------------------------------------------------------------------
    // Save
    // -------------------------------------------------------------------------

    /// <summary>
    /// Serializes and writes <paramref name="data"/> to the farmer's
    /// modData.
    /// </summary>
    public void Save(Farmer farmer, SaveDataModel data)
    {
        try
        {
            var json = JsonConvert.SerializeObject(data, Formatting.None);
            farmer.modData[SaveDataKey] = json;
            _log.Trace("Save data written for '{0}'.", farmer.Name);
        }
        catch (Exception ex)
        {
            _log.Exception(
                $"Failed to serialize save data for '{farmer.Name}'", ex);
        }
    }

    /// <summary>
    /// Converts a <see cref="PlayerRegressionState"/> to a
    /// <see cref="SaveDataModel"/> and saves it for the given farmer.
    /// </summary>
    public void SaveFromState(Farmer farmer, PlayerRegressionState state)
    {
        var data = BuildSaveData(state, AbsoluteDayHelper.GetCurrentAbsoluteDay());
        Save(farmer, data);
    }

    // -------------------------------------------------------------------------
    // Shared builder
    // -------------------------------------------------------------------------

    /// <summary>
    /// Converts a <see cref="PlayerRegressionState"/> into a
    /// <see cref="SaveDataModel"/> at the given absolute day.
    ///
    /// <para>
    /// This method is <c>public static</c> so unit tests can call it
    /// directly without a live <see cref="Farmer"/> instance, eliminating
    /// the need to duplicate the serialization logic in test helpers.
    /// Pass <c>0</c> for <paramref name="absoluteDay"/> in tests where
    /// the day value is not under test.
    /// </para>
    /// </summary>
    /// <param name="state">The player state to serialize.</param>
    /// <param name="absoluteDay">
    /// The current absolute day number, used for
    /// <see cref="SaveDataModel.LastSavedAbsoluteDay"/>.
    /// </param>
    public static SaveDataModel BuildSaveData(
        PlayerRegressionState state,
        int absoluteDay)
    {
        return new SaveDataModel
        {
            SchemaVersion            = SaveDataMigrator.CurrentSchemaVersion,
            CurrentStageId           = state.CurrentStageId,
            EquippedDiaperTypeId     = state.Diaper.EquippedDiaperTypeId,
            DiaperWetness            = state.Diaper.WetnessLevel,
            DiaperMessing            = state.Diaper.MessingLevel,
            DiaperHasBooster         = state.Diaper.HasBooster,
            DiaperLastChangedAbsoluteDay = state.Diaper.LastChangedAbsoluteDay,
            ComfortScore             = state.Comfort.CurrentComfort,
            EquippedAccessories      = state.EquippedAccessories.ToList(),
            DialogueCooldowns        = new Dictionary<string, int>(
                state.DialogueCooldowns),
            LastSavedAbsoluteDay     = absoluteDay,
            // v2 — needs
            ContinenceNormalized     = state.Needs.Continence.Value.Normalized,
            ContinenceLastThresholdId =
                state.Needs.Continence.Value.LastKnownThresholdId,
            ContinenceStressModifier = state.Needs.Continence.StressModifier,
            HungerNormalized         = state.Needs.Hunger.Normalized,
            HungerLastThresholdId    = state.Needs.Hunger.LastKnownThresholdId,
            ThirstNormalized         = state.Needs.Thirst.Normalized,
            ThirstLastThresholdId    = state.Needs.Thirst.LastKnownThresholdId,
            // additive fields
            CurrentMoodId            = state.Mood.CurrentMoodId,
            SpouseDailyDialogueLastGivenAbsoluteDay =
                state.SpouseDailyDialogueLastGivenAbsoluteDay
        };
    }

    // -------------------------------------------------------------------------
    // Deserialization
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reconstructs a <see cref="PlayerRegressionState"/> from a
    /// <see cref="SaveDataModel"/>.
    /// </summary>
    public static PlayerRegressionState StateFromSaveData(
        SaveDataModel data,
        long playerId)
    {
        var diaperState = data.EquippedDiaperTypeId is not null
            ? new DiaperState
            {
                EquippedDiaperTypeId   = data.EquippedDiaperTypeId,
                WetnessLevel           = data.DiaperWetness,
                MessingLevel           = data.DiaperMessing,
                HasBooster             = data.DiaperHasBooster,
                LastChangedAbsoluteDay = data.DiaperLastChangedAbsoluteDay
            }
            : DiaperState.None;

        return new PlayerRegressionState
        {
            PlayerId       = playerId,
            CurrentStageId = data.CurrentStageId,
            Diaper         = diaperState,
            Comfort        = new ComfortState
            {
                CurrentComfort = data.ComfortScore
            },
            Needs = new PlayerNeedsState
            {
                Continence = new ContinenceState
                {
                    Value = new NeedsValue
                    {
                        Normalized           = data.ContinenceNormalized,
                        LastKnownThresholdId = data.ContinenceLastThresholdId
                    },
                    StressModifier = data.ContinenceStressModifier
                },
                Hunger = new NeedsValue
                {
                    Normalized           = data.HungerNormalized,
                    LastKnownThresholdId = data.HungerLastThresholdId
                },
                Thirst = new NeedsValue
                {
                    Normalized           = data.ThirstNormalized,
                    LastKnownThresholdId = data.ThirstLastThresholdId
                }
            },
            Mood = new MoodState
            {
                CurrentMoodId = data.CurrentMoodId
            },
            EquippedAccessories  = new HashSet<string>(data.EquippedAccessories),
            DialogueCooldowns    = new Dictionary<string, int>(
                data.DialogueCooldowns),
            LastUpdatedAbsoluteDay = data.LastSavedAbsoluteDay,
            SpouseDailyDialogueLastGivenAbsoluteDay =
                data.SpouseDailyDialogueLastGivenAbsoluteDay
        };
    }
}
