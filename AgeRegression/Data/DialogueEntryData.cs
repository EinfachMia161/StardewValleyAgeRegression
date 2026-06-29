namespace AgeRegression.Data;

/// <summary>
/// A single dialogue entry in the data-driven dialogue system.
/// Entries are loaded from JSON files under <c>assets/dialogue/</c>.
/// </summary>
public sealed class DialogueEntryData
{
    /// <summary>
    /// Unique key within this dialogue file. Used for cooldown tracking
    /// and deduplication when packs are merged.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The dialogue text shown to the player.
    /// Supports <c>{tokens}</c> resolved by
    /// <see cref="AgeRegression.Dialogue.DialogueTokenResolver"/>.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Conditions that must all be satisfied for this entry to be
    /// eligible. A <c>null</c> conditions object means the entry is
    /// always eligible.
    /// </summary>
    public DialogueConditions? Conditions { get; set; }

    /// <summary>
    /// Relative weight for random selection among eligible entries.
    /// Higher values make this entry more likely to be chosen.
    /// Default is 1.
    /// </summary>
    public int Weight { get; set; } = 1;

    /// <summary>
    /// Cooldown in in-game days before this specific entry can be
    /// shown again. 0 means no cooldown.
    /// </summary>
    public int CooldownDays { get; set; } = 0;
}

/// <summary>
/// Conditions that must be satisfied for a
/// <see cref="DialogueEntryData"/> to be eligible for selection.
/// All non-null fields are evaluated as AND conditions.
/// A null field means no restriction on that dimension.
/// </summary>
public sealed class DialogueConditions
{
    // --- Regression ---

    /// <summary>
    /// Required regression stage ID(s).
    /// <c>null</c> matches any stage.
    /// </summary>
    public List<string>? RegressionStages { get; set; }

    // --- Friendship ---

    /// <summary>
    /// Minimum friendship hearts required (0–14).
    /// <c>null</c> means no minimum.
    /// </summary>
    public int? MinFriendshipHearts { get; set; }

    /// <summary>
    /// Maximum friendship hearts allowed.
    /// <c>null</c> means no maximum.
    /// </summary>
    public int? MaxFriendshipHearts { get; set; }

    /// <summary>
    /// Whether the player must be married to this NPC.
    /// <c>null</c> means marriage status is not checked.
    /// </summary>
    public bool? IsMarried { get; set; }

    // --- World ---

    /// <summary>
    /// Required season(s): spring, summer, fall, winter.
    /// <c>null</c> matches any season.
    /// </summary>
    public List<string>? Seasons { get; set; }

    /// <summary>
    /// Required time range lower bound (inclusive).
    /// Uses Stardew time format (e.g. 600, 1200, 2000).
    /// <c>null</c> means no lower bound.
    /// </summary>
    public int? TimeFrom { get; set; }

    /// <summary>
    /// Required time range upper bound (inclusive).
    /// <c>null</c> means no upper bound.
    /// </summary>
    public int? TimeTo { get; set; }

    /// <summary>
    /// Required weather: sunny, rainy, stormy, snowy, windy.
    /// <c>null</c> matches any weather.
    /// </summary>
    public List<string>? Weather { get; set; }

    /// <summary>
    /// Required location name(s) the player must be in.
    /// <c>null</c> matches any location.
    /// </summary>
    public List<string>? Locations { get; set; }

    // --- Diaper ---

    /// <summary>
    /// Required diaper condition(s): clean, damp, wet, soaked.
    /// <c>null</c> means diaper condition is not checked.
    /// </summary>
    public List<string>? DiaperConditions { get; set; }

    /// <summary>
    /// Whether the player must be wearing a diaper.
    /// <c>null</c> means this is not checked.
    /// </summary>
    public bool? IsWearingDiaper { get; set; }

    /// <summary>
    /// Required equipped diaper type ID(s). Used for comfort modifiers.
    /// All listed diapers must match the currently equipped diaper.
    /// <c>null</c> means diaper type is not checked.
    /// </summary>
    public List<string>? EquippedDiaperIds { get; set; }

    // --- Accessories ---

    /// <summary>
    /// Required equipped accessory IDs (pacifier, bottle, etc.).
    /// All listed accessories must be equipped.
    /// <c>null</c> means accessories are not checked.
    /// </summary>
    public List<string>? RequiredAccessories { get; set; }

    // --- NPC ---

    /// <summary>
    /// NPC personality tags that must be present on the NPC's profile.
    /// Any one matching tag satisfies this condition.
    /// <c>null</c> means personality is not checked.
    /// </summary>
    public List<string>? NpcPersonalityTags { get; set; }

    // --- Game state ---

    /// <summary>
    /// Game state flags that must be set (e.g. completed quests,
    /// seen events). Uses Stardew's mail/flag system prefixed with
    /// <c>event_</c> for event IDs.
    /// <c>null</c> means no flags required.
    /// </summary>
    public List<string>? RequiredGameFlags { get; set; }

    // --- Needs ---

    /// <summary>
    /// Required continence threshold ID(s).
    /// <c>null</c> means continence threshold is not checked.
    /// </summary>
    public List<string>? ContinenceThresholds { get; set; }

    /// <summary>
    /// Minimum normalized hunger value (0.0–1.0).
    /// <c>null</c> means no minimum.
    /// </summary>
    public float? MinHungerNormalized { get; set; }

    /// <summary>
    /// Maximum normalized hunger value (0.0–1.0).
    /// <c>null</c> means no maximum.
    /// </summary>
    public float? MaxHungerNormalized { get; set; }

    /// <summary>
    /// Minimum normalized thirst value (0.0–1.0).
    /// <c>null</c> means no minimum.
    /// </summary>
    public float? MinThirstNormalized { get; set; }

    /// <summary>
    /// Maximum normalized thirst value (0.0–1.0).
    /// <c>null</c> means no maximum.
    /// </summary>
    public float? MaxThirstNormalized { get; set; }

    /// <summary>
    /// Minimum normalized comfort value (0.0–1.0).
    /// <c>null</c> means no minimum.
    /// </summary>
    public float? MinComfortNormalized { get; set; }

    /// <summary>
    /// Maximum normalized comfort value (0.0–1.0).
    /// <c>null</c> means no maximum.
    /// </summary>
    public float? MaxComfortNormalized { get; set; }

    // --- Care ---

    /// <summary>
    /// Required care action ID(s) that was last performed.
    /// <c>null</c> means care action is not checked.
    /// </summary>
    public List<string>? CareActionIds { get; set; }

    /// <summary>
    /// Minimum number of care actions performed today.
    /// <c>null</c> means no minimum.
    /// </summary>
    public int? MinCareActionsToday { get; set; }

    /// <summary>
    /// Maximum number of care actions performed today.
    /// <c>null</c> means no maximum.
    /// </summary>
    public int? MaxCareActionsToday { get; set; }

    /// <summary>
    /// Maximum days since last diaper change.
    /// <c>null</c> means no restriction.
    /// </summary>
    public int? MaxDaysSinceLastDiaperChange { get; set; }
}