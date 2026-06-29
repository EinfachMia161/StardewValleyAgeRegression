namespace AgeRegression.Persistence;

/// <summary>
/// Versioned save data schema. All data persisted to the save file
/// lives here, serialized as a single JSON blob in
/// <c>farmer.modData</c> under the key
/// <c>mia.AgeRegression/SaveData</c>.
///
/// <para>
/// Versioning contract:
/// <list type="bullet">
///   <item>Always provide default values for new fields so that saves
///   created before the field existed deserialize correctly.</item>
///   <item>Increment <see cref="SchemaVersion"/> and add a migration
///   step in <see cref="SaveDataMigrator"/> for any breaking semantic
///   change.</item>
///   <item>Additive fields (new fields with safe defaults) do not
///   require a schema version bump.</item>
/// </list>
/// </summary>
///
/// <para>
/// Schema history:
/// <list type="bullet">
///   <item>v1 — Initial schema.</item>
///   <item>v2 — Added needs state (continence, hunger, thirst).</item>
///   <item>v3 — Added care state.</item>
/// </list>
/// </summary>
public sealed class SaveDataModel
{
    // -------------------------------------------------------------------------
    // Schema version
    // -------------------------------------------------------------------------

    /// <summary>
    /// Schema version. Increment on breaking changes.
    /// Current version: 3.
    /// </summary>
    public int SchemaVersion { get; set; } = 3;

    // -------------------------------------------------------------------------
    // v1 fields — regression and diaper
    // -------------------------------------------------------------------------

    /// <summary>The player's current regression stage ID.</summary>
    public string CurrentStageId { get; set; } = "none";

    /// <summary>
    /// The type ID of the currently equipped diaper, or <c>null</c>
    /// if no diaper is equipped.
    /// </summary>
    public string? EquippedDiaperTypeId { get; set; }

    /// <summary>Current diaper wetness level (0.0–1.0).</summary>
    public float DiaperWetness { get; set; } = 0f;

    /// <summary>Current diaper messing level (0.0–1.0).</summary>
    public float DiaperMessing { get; set; } = 0f;

    /// <summary>Whether a booster insert is in the current diaper.</summary>
    public bool DiaperHasBooster { get; set; } = false;

    /// <summary>
    /// Absolute day number when the diaper was last changed.
    /// Computed via <see cref="Utilities.AbsoluteDayHelper"/>.
    /// </summary>
    public int DiaperLastChangedAbsoluteDay { get; set; } = 0;

    /// <summary>Current comfort score.</summary>
    public float ComfortScore { get; set; } = 50f;

    /// <summary>Currently equipped accessory IDs.</summary>
    public List<string> EquippedAccessories { get; set; } = new();

    /// <summary>
    /// Dialogue cooldown tracking.
    /// Key: <c>"{npcName}:{dialogueKey}"</c>.
    /// Value: absolute day number when the entry was last shown.
    /// </summary>
    public Dictionary<string, int> DialogueCooldowns { get; set; } = new();

    /// <summary>
    /// Absolute day number when this data was last saved.
    /// </summary>
    public int LastSavedAbsoluteDay { get; set; } = 0;

    // -------------------------------------------------------------------------
    // v2 fields — needs state
    // -------------------------------------------------------------------------

    /// <summary>Normalized continence value (0.0–1.0).</summary>
    public float ContinenceNormalized { get; set; } = 1.0f;

    /// <summary>Last known continence threshold ID.</summary>
    public string ContinenceLastThresholdId { get; set; } = string.Empty;

    /// <summary>Continence stress modifier.</summary>
    public float ContinenceStressModifier { get; set; } = 0f;

    /// <summary>Normalized hunger value (0.0–1.0).</summary>
    public float HungerNormalized { get; set; } = 1.0f;

    /// <summary>Last known hunger threshold ID.</summary>
    public string HungerLastThresholdId { get; set; } = string.Empty;

    /// <summary>Normalized thirst value (0.0–1.0).</summary>
    public float ThirstNormalized { get; set; } = 1.0f;

    /// <summary>Last known thirst threshold ID.</summary>
    public string ThirstLastThresholdId { get; set; } = string.Empty;

    // -------------------------------------------------------------------------
    // v3 fields — care state
    // -------------------------------------------------------------------------

    /// <summary>
    /// Absolute day number of the last diaper change.
    /// Used for dialogue context.
    /// </summary>
    public int LastDiaperChangeAbsoluteDay { get; set; } = 0;

    /// <summary>
    /// Number of care actions performed today. Resets at day start.
    /// </summary>
    public int CareActionsToday { get; set; } = 0;

    /// <summary>
    /// The ID of the last care action performed.
    /// </summary>
    public string LastCareActionId { get; set; } = string.Empty;

    /// <summary>
    /// Absolute day number when CareActionsToday was last reset.
    /// </summary>
    public int LastCareDayAbsolute { get; set; } = 0;

    // -------------------------------------------------------------------------
    // Additive fields (no schema bump required — safe defaults)
    // -------------------------------------------------------------------------

    /// <summary>Current mood level ID.</summary>
    public string CurrentMoodId { get; set; } = string.Empty;

    /// <summary>
    /// Absolute day number when the spouse last gave daily regression
    /// dialogue. 0 means never.
    /// </summary>
    public int SpouseDailyDialogueLastGivenAbsoluteDay { get; set; } = 0;
}