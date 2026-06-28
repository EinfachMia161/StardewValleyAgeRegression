using AgeRegression.Utilities;
using Newtonsoft.Json;

namespace AgeRegression.Data;

/// <summary>
/// Loads and caches all data assets for the mod.
/// Has no direct dependency on the file system — all I/O is delegated
/// to <see cref="IAssetProvider"/>.
/// </summary>
public sealed class DataLoader
{
    private readonly IAssetProvider _assets;
    private readonly LogHelper _log;

    // -------------------------------------------------------------------------
    // Cached collections
    // -------------------------------------------------------------------------

    private List<RegressionStageData>    _stages           = new();
    private List<DiaperTypeData>         _diaperTypes      = new();
    private List<NpcReactionProfileData> _npcProfiles      = new();
    private List<WardrobeItemData>       _wardrobeItems    = new();
    private List<NeedsThresholdSetData>  _needsThresholds  = new();
    private List<ComfortModifierData>    _comfortModifiers = new();
    private List<MoodLevelData>          _moodLevels       = new();
    private List<FurnitureItemData>      _furnitureItems   = new();
    private List<FurnitureComfortData>   _furnitureComfort = new();
    private List<EventDefinitionData>    _eventDefinitions = new();

    private readonly Dictionary<string, DialoguePackData> _dialoguePacks = new();

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public DataLoader(IAssetProvider assets, LogHelper log)
    {
        _assets = assets;
        _log    = log;
    }

    // -------------------------------------------------------------------------
    // Public accessors
    // -------------------------------------------------------------------------

    /// <summary>
    /// All loaded regression stages, sorted by
    /// <see cref="RegressionStageData.Order"/>.
    /// </summary>
    public IReadOnlyList<RegressionStageData> Stages => _stages;

    /// <summary>All loaded diaper type definitions.</summary>
    public IReadOnlyList<DiaperTypeData> DiaperTypes => _diaperTypes;

    /// <summary>All loaded NPC reaction profiles.</summary>
    public IReadOnlyList<NpcReactionProfileData> NpcProfiles => _npcProfiles;

    /// <summary>All loaded wardrobe accessory definitions.</summary>
    public IReadOnlyList<WardrobeItemData> WardrobeItems => _wardrobeItems;

    /// <summary>All loaded needs threshold set definitions.</summary>
    public IReadOnlyList<NeedsThresholdSetData> NeedsThresholdSets => _needsThresholds;

    /// <summary>All loaded comfort modifier definitions.</summary>
    public IReadOnlyList<ComfortModifierData> ComfortModifiers => _comfortModifiers;

    /// <summary>All loaded mood level definitions.</summary>
    public IReadOnlyList<MoodLevelData> MoodLevels => _moodLevels;

    /// <summary>All loaded furniture item definitions.</summary>
    public IReadOnlyList<FurnitureItemData> FurnitureItems => _furnitureItems;

    /// <summary>All loaded furniture comfort mappings.</summary>
    public IReadOnlyList<FurnitureComfortData> FurnitureComfort => _furnitureComfort;

    /// <summary>All loaded event definitions.</summary>
    public IReadOnlyList<EventDefinitionData> EventDefinitions => _eventDefinitions;

    // -------------------------------------------------------------------------
    // Lookup helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the dialogue pack for the given key, or <c>null</c>.
    /// </summary>
    public DialoguePackData? GetDialoguePack(string packKey) =>
        _dialoguePacks.TryGetValue(packKey, out var pack) ? pack : null;

    /// <summary>
    /// Returns the stage with the given ID, or <c>null</c>.
    /// </summary>
    public RegressionStageData? GetStage(string stageId) =>
        _stages.FirstOrDefault(s => s.Id == stageId);

    /// <summary>
    /// Returns the diaper type with the given ID, or <c>null</c>.
    /// </summary>
    public DiaperTypeData? GetDiaperType(string typeId) =>
        _diaperTypes.FirstOrDefault(d => d.Id == typeId);

    /// <summary>
    /// Returns the wardrobe item with the given ID, or <c>null</c>.
    /// </summary>
    public WardrobeItemData? GetWardrobeItem(string itemId) =>
        _wardrobeItems.FirstOrDefault(w => w.Id == itemId);

    /// <summary>
    /// Returns the mood level with the given ID, or <c>null</c>.
    /// </summary>
    public MoodLevelData? GetMoodLevel(string moodId) =>
        _moodLevels.FirstOrDefault(m => m.Id == moodId);

    /// <summary>
    /// Returns the stage with the lowest
    /// <see cref="RegressionStageData.Order"/> (the baseline /
    /// no-regression stage).
    /// </summary>
    public RegressionStageData? GetBaselineStage() =>
        _stages.Count > 0 ? _stages[0] : null;

    // -------------------------------------------------------------------------
    // Loading
    // -------------------------------------------------------------------------

    /// <summary>
    /// Loads all data assets. Safe to call multiple times — reloads on
    /// each call.
    /// </summary>
    public void LoadAll()
    {
        _log.Debug("Loading all data assets...");

        LoadStages();
        LoadDiaperTypes();
        LoadNpcProfiles();
        LoadWardrobeItems();
        LoadNeedsThresholds();
        LoadComfortModifiers();
        LoadMoodLevels();
        LoadFurnitureItems();
        LoadFurnitureComfort();
        LoadEventDefinitions();
        LoadDialoguePacks();

        _log.Info(
            "Data loaded: {0} stages, {1} diaper types, {2} NPC profiles, " +
            "{3} wardrobe items, {4} comfort modifiers, {5} mood levels, " +
            "{6} furniture items, {7} event definitions, {8} dialogue packs.",
            _stages.Count, _diaperTypes.Count, _npcProfiles.Count,
            _wardrobeItems.Count, _comfortModifiers.Count, _moodLevels.Count,
            _furnitureItems.Count, _eventDefinitions.Count, _dialoguePacks.Count);
    }

    // -------------------------------------------------------------------------
    // Private loaders
    // -------------------------------------------------------------------------

    private void LoadStages()
    {
        var loaded = _assets.Load<List<RegressionStageData>>(
            "assets/data/regression-stages.json");

        if (loaded is null || loaded.Count == 0)
        {
            _log.Warn("No regression stages found. Using built-in defaults.");
            loaded = GetDefaultStages();
        }

        var seen   = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var deduped = new List<RegressionStageData>();

        foreach (var stage in loaded)
        {
            if (string.IsNullOrWhiteSpace(stage.Id))
            {
                _log.Warn("Regression stage with empty ID — skipping.");
                continue;
            }

            if (!seen.Add(stage.Id))
            {
                _log.Warn("Duplicate stage ID '{0}' — keeping first occurrence.",
                    stage.Id);
                continue;
            }

            deduped.Add(stage);
        }

        _stages = deduped.OrderBy(s => s.Order).ToList();
    }

    private void LoadDiaperTypes()
    {
        var loaded = _assets.Load<List<DiaperTypeData>>(
            "assets/data/diaper-types.json");

        if (loaded is null || loaded.Count == 0)
        {
            _log.Warn("No diaper types found. Diaper system will be inactive.");
            _diaperTypes = new List<DiaperTypeData>();
            return;
        }

        _diaperTypes = loaded;
    }

    private void LoadNpcProfiles()
    {
        var loaded = _assets.Load<List<NpcReactionProfileData>>(
            "assets/data/npc-reactions.json");
        _npcProfiles = loaded ?? new List<NpcReactionProfileData>();
    }

    private void LoadWardrobeItems()
    {
        var loaded = _assets.Load<List<WardrobeItemData>>(
            "assets/data/wardrobe-items.json");

        if (loaded is null || loaded.Count == 0)
        {
            _log.Warn("No wardrobe items found. Wardrobe system will have no items.");
            _wardrobeItems = new List<WardrobeItemData>();
            return;
        }

        _wardrobeItems = loaded;
    }

    private void LoadNeedsThresholds()
    {
        var loaded = _assets.Load<List<NeedsThresholdSetData>>(
            "assets/data/needs-thresholds.json");

        if (loaded is null || loaded.Count == 0)
        {
            _log.Warn("No needs thresholds found. Using built-in defaults.");
            _needsThresholds = GetDefaultNeedsThresholds();
            return;
        }

        _needsThresholds = loaded;
    }

    private void LoadComfortModifiers()
    {
        var loaded = _assets.Load<List<ComfortModifierData>>(
            "assets/data/comfort-modifiers.json");

        if (loaded is null || loaded.Count == 0)
        {
            _log.Warn("No comfort modifiers found. Using built-in defaults.");
            _comfortModifiers = GetDefaultComfortModifiers();
            return;
        }

        _comfortModifiers = loaded;
    }

    private void LoadMoodLevels()
    {
        var loaded = _assets.Load<List<MoodLevelData>>(
            "assets/data/mood-levels.json");

        if (loaded is null || loaded.Count == 0)
        {
            _log.Warn("No mood levels found. Using built-in defaults.");
            _moodLevels = GetDefaultMoodLevels();
            return;
        }

        _moodLevels = loaded.OrderBy(m => m.MinComfortNormalized).ToList();
    }

    private void LoadFurnitureItems()
    {
        var loaded = _assets.Load<List<FurnitureItemData>>(
            "assets/data/furniture-items.json");
        _furnitureItems = loaded ?? new List<FurnitureItemData>();
    }

    private void LoadFurnitureComfort()
    {
        var loaded = _assets.Load<List<FurnitureComfortData>>(
            "assets/data/furniture-comfort.json");
        _furnitureComfort = loaded ?? new List<FurnitureComfortData>();
    }

    private void LoadEventDefinitions()
    {
        var loaded = _assets.Load<List<EventDefinitionData>>(
            "assets/data/events.json");
        _eventDefinitions = loaded ?? new List<EventDefinitionData>();
        _log.Debug("Loaded {0} event definitions.", _eventDefinitions.Count);
    }

    private void LoadDialoguePacks()
    {
        _dialoguePacks.Clear();

        foreach (var filePath in _assets.EnumerateFiles(
                     "assets/dialogue", "*.json", recursive: true))
        {
            LoadDialogueFile(filePath);
        }
    }

    private void LoadDialogueFile(string filePath)
    {
        var raw = _assets.LoadRaw(filePath);
        if (raw is null)
            return;

        DialoguePackData? pack;
        try
        {
            pack = JsonConvert.DeserializeObject<DialoguePackData>(raw);
        }
        catch (Exception ex)
        {
            _log.Exception(
                $"Failed to deserialize dialogue file '{Path.GetFileName(filePath)}'",
                ex);
            return;
        }

        if (pack is null)
        {
            _log.Warn("Dialogue file '{0}' deserialized to null — skipping.",
                Path.GetFileName(filePath));
            return;
        }

        if (string.IsNullOrWhiteSpace(pack.PackId))
        {
            _log.Warn("Dialogue file '{0}' has no PackId — skipping.",
                Path.GetFileName(filePath));
            return;
        }

        if (_dialoguePacks.TryGetValue(pack.PackId, out var existing))
        {
            existing.Entries.AddRange(pack.Entries);
            _log.Debug("Merged {0} entries into pack '{1}' from '{2}'.",
                pack.Entries.Count, pack.PackId, Path.GetFileName(filePath));
        }
        else
        {
            _dialoguePacks[pack.PackId] = pack;
            _log.Debug("Loaded dialogue pack '{0}' ({1} entries) from '{2}'.",
                pack.PackId, pack.Entries.Count, Path.GetFileName(filePath));
        }
    }

    // -------------------------------------------------------------------------
    // Built-in defaults
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the built-in default regression stages used when the data
    /// file is missing. Internal so tests can access it directly.
    /// </summary>
    internal static List<RegressionStageData> GetDefaultStages() => new()
    {
        new RegressionStageData
        {
            Id           = "none",
            DisplayName  = "Normal",
            Order        = 0,
            Description  = "No regression. The player behaves normally.",
            StatModifiers = new StageStatModifiers()
        },
        new RegressionStageData
        {
            Id           = "little",
            DisplayName  = "Little",
            Order        = 1,
            Description  = "Mild regression. Slight stat changes.",
            StatModifiers = new StageStatModifiers
            {
                SpeedMultiplier    = 0.95f,
                MaxEnergyMultiplier = 0.95f
            }
        },
        new RegressionStageData
        {
            Id           = "middle",
            DisplayName  = "Middle",
            Order        = 2,
            Description  = "Moderate regression.",
            StatModifiers = new StageStatModifiers
            {
                SpeedMultiplier    = 0.85f,
                MaxEnergyMultiplier = 0.85f,
                SkillXpMultiplier  = 0.9f
            }
        },
        new RegressionStageData
        {
            Id           = "baby",
            DisplayName  = "Baby",
            Order        = 3,
            Description  = "Deep regression. Significant stat changes.",
            StatModifiers = new StageStatModifiers
            {
                SpeedMultiplier    = 0.7f,
                MaxEnergyMultiplier = 0.7f,
                SkillXpMultiplier  = 0.75f,
                CanUseTools        = false
            }
        }
    };

    internal static List<NeedsThresholdSetData> GetDefaultNeedsThresholds() => new()
    {
        new NeedsThresholdSetData
        {
            NeedId = "continence",
            Thresholds = new List<NeedsThresholdData>
            {
                new() { Id = "loss_of_control", DisplayName = "Loss of Control",
                        MinNormalized = 0.00f, Order = 0 },
                new() { Id = "struggling",      DisplayName = "Struggling",
                        MinNormalized = 0.15f, Order = 1 },
                new() { Id = "warning",         DisplayName = "Warning",
                        MinNormalized = 0.35f, Order = 2 },
                new() { Id = "comfortable",     DisplayName = "Comfortable",
                        MinNormalized = 0.60f, Order = 3 }
            }
        },
        new NeedsThresholdSetData
        {
            NeedId = "hunger",
            Thresholds = new List<NeedsThresholdData>
            {
                new() { Id = "starving",  DisplayName = "Starving",
                        MinNormalized = 0.00f, Order = 0 },
                new() { Id = "hungry",    DisplayName = "Hungry",
                        MinNormalized = 0.20f, Order = 1 },
                new() { Id = "peckish",   DisplayName = "Peckish",
                        MinNormalized = 0.45f, Order = 2 },
                new() { Id = "satisfied", DisplayName = "Satisfied",
                        MinNormalized = 0.70f, Order = 3 }
            }
        },
        new NeedsThresholdSetData
        {
            NeedId = "thirst",
            Thresholds = new List<NeedsThresholdData>
            {
                new() { Id = "dehydrated", DisplayName = "Dehydrated",
                        MinNormalized = 0.00f, Order = 0 },
                new() { Id = "thirsty",    DisplayName = "Thirsty",
                        MinNormalized = 0.20f, Order = 1 },
                new() { Id = "dry",        DisplayName = "Dry",
                        MinNormalized = 0.45f, Order = 2 },
                new() { Id = "hydrated",   DisplayName = "Hydrated",
                        MinNormalized = 0.70f, Order = 3 }
            }
        }
    };

    internal static List<ComfortModifierData> GetDefaultComfortModifiers() => new()
    {
        new ComfortModifierData
        {
            Id = "diaper_clean", Description = "Wearing a clean diaper",
            ValuePerHour = 3f, ImmediateValue = 0f,
            Conditions = new DialogueConditions
            {
                IsWearingDiaper  = true,
                DiaperConditions = new List<string> { "clean" }
            },
            Priority = 10
        },
        new ComfortModifierData
        {
            Id = "diaper_damp", Description = "Wearing a damp diaper",
            ValuePerHour = -2f, ImmediateValue = 0f,
            Conditions = new DialogueConditions
            {
                IsWearingDiaper  = true,
                DiaperConditions = new List<string> { "damp" }
            },
            Priority = 10
        },
        new ComfortModifierData
        {
            Id = "diaper_wet", Description = "Wearing a wet diaper",
            ValuePerHour = -6f, ImmediateValue = -5f,
            Conditions = new DialogueConditions
            {
                IsWearingDiaper  = true,
                DiaperConditions = new List<string> { "wet" }
            },
            Priority = 10
        },
        new ComfortModifierData
        {
            Id = "diaper_soaked", Description = "Wearing a soaked diaper",
            ValuePerHour = -15f, ImmediateValue = -10f,
            Conditions = new DialogueConditions
            {
                IsWearingDiaper  = true,
                DiaperConditions = new List<string> { "soaked" }
            },
            Priority = 10
        },
        new ComfortModifierData
        {
            Id = "pacifier_equipped", Description = "Wearing a pacifier",
            ValuePerHour = 4f, ImmediateValue = 3f,
            Conditions = new DialogueConditions
            {
                RequiredAccessories = new List<string> { "pacifier" }
            },
            Priority = 5
        },
        new ComfortModifierData
        {
            Id = "plushie_equipped", Description = "Carrying a plushie",
            ValuePerHour = 3f, ImmediateValue = 2f,
            Conditions = new DialogueConditions
            {
                RequiredAccessories = new List<string> { "plushie_bunny" }
            },
            Priority = 5
        },
        new ComfortModifierData
        {
            Id = "continence_struggling", Description = "Struggling with continence",
            ValuePerHour = -8f, ImmediateValue = -5f,
            Conditions = new DialogueConditions
            {
                ContinenceThresholds = new List<string> { "struggling" }
            },
            Priority = 8
        },
        new ComfortModifierData
        {
            Id = "continence_loss_of_control",
            Description = "Lost continence control",
            ValuePerHour = -20f, ImmediateValue = -15f,
            Conditions = new DialogueConditions
            {
                ContinenceThresholds = new List<string> { "loss_of_control" }
            },
            Priority = 9
        },
        new ComfortModifierData
        {
            Id = "hungry", Description = "Feeling hungry",
            ValuePerHour = -4f, ImmediateValue = 0f,
            Conditions = new DialogueConditions { MaxHungerNormalized = 0.3f },
            Priority = 6
        },
        new ComfortModifierData
        {
            Id = "thirsty", Description = "Feeling thirsty",
            ValuePerHour = -4f, ImmediateValue = 0f,
            Conditions = new DialogueConditions { MaxThirstNormalized = 0.3f },
            Priority = 6
        },
        new ComfortModifierData
        {
            Id = "passive_decay_regressed",
            Description = "Passive comfort decay while regressed",
            ValuePerHour = -2f, ImmediateValue = 0f,
            Conditions = new DialogueConditions
            {
                RegressionStages = new List<string> { "little", "middle", "baby" }
            },
            Priority = 1
        }
    };

    internal static List<MoodLevelData> GetDefaultMoodLevels() => new()
    {
        new MoodLevelData
        {
            Id = "distressed", DisplayName = "Distressed",
            MinComfortNormalized = 0.00f, Order = 0,
            StatModifiers = new MoodStatModifiers
            {
                SpeedMultiplier   = 0.85f,
                SkillXpMultiplier = 0.75f
            }
        },
        new MoodLevelData
        {
            Id = "fussy", DisplayName = "Fussy",
            MinComfortNormalized = 0.20f, Order = 1,
            StatModifiers = new MoodStatModifiers
            {
                SpeedMultiplier   = 0.92f,
                SkillXpMultiplier = 0.88f
            }
        },
        new MoodLevelData
        {
            Id = "okay", DisplayName = "Okay",
            MinComfortNormalized = 0.45f, Order = 2,
            StatModifiers = new MoodStatModifiers()
        },
        new MoodLevelData
        {
            Id = "content", DisplayName = "Content",
            MinComfortNormalized = 0.65f, Order = 3,
            StatModifiers = new MoodStatModifiers
            {
                SpeedMultiplier   = 1.02f,
                SkillXpMultiplier = 1.05f
            }
        },
        new MoodLevelData
        {
            Id = "happy", DisplayName = "Happy",
            MinComfortNormalized = 0.85f, Order = 4,
            StatModifiers = new MoodStatModifiers
            {
                SpeedMultiplier   = 1.05f,
                SkillXpMultiplier = 1.10f
            }
        }
    };
}
