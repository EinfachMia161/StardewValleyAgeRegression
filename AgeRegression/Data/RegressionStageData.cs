namespace AgeRegression.Data;

/// <summary>
/// Represents a single regression stage as loaded from
/// <c>assets/data/regression-stages.json</c>.
/// </summary>
public sealed class RegressionStageData
{
    /// <summary>
    /// Unique identifier for this stage. Used as the key throughout
    /// the codebase. Example: <c>"little"</c>.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable display name shown in UI.
    /// Example: <c>"Little"</c>.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Sort order. Lower values are less regressed.
    /// The stage with the lowest order is treated as the baseline
    /// (no regression) stage.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Optional description shown in tooltips or menus.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Stat modifiers applied while the player is at this stage.
    /// All values are multipliers (1.0 = no change, 0.8 = 20% reduction).
    /// </summary>
    public StageStatModifiers StatModifiers { get; set; } = new();
}

/// <summary>
/// Stat multipliers applied to the player at a given regression stage.
/// All values default to 1.0 (no effect).
/// </summary>
public sealed class StageStatModifiers
{
    /// <summary>Movement speed multiplier. 1.0 = normal speed.</summary>
    public float SpeedMultiplier { get; set; } = 1.0f;

    /// <summary>
    /// Maximum energy multiplier. 1.0 = normal energy cap.
    /// </summary>
    public float MaxEnergyMultiplier { get; set; } = 1.0f;

    /// <summary>
    /// Skill XP gain multiplier. 1.0 = normal XP gain.
    /// </summary>
    public float SkillXpMultiplier { get; set; } = 1.0f;

    /// <summary>
    /// Whether the player can use tools at this stage.
    /// Intended for the deepest regression stages only.
    /// </summary>
    public bool CanUseTools { get; set; } = true;
}
