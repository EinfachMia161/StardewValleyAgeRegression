namespace AgeRegression.Data;

/// <summary>
/// Defines a named mood level derived from comfort score.
/// Loaded from <c>assets/data/mood-levels.json</c>.
/// </summary>
public sealed class MoodLevelData
{
    /// <summary>
    /// Unique identifier. Example: <c>"content"</c>, <c>"fussy"</c>,
    /// <c>"distressed"</c>, <c>"happy"</c>.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Minimum normalized comfort value (0–1) for this mood to be active.
    /// </summary>
    public float MinComfortNormalized { get; set; } = 0f;

    /// <summary>Sort order (lower = worse mood).</summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// Optional stat modifier applied while in this mood.
    /// Stacks multiplicatively with regression stage modifiers via
    /// <see cref="Systems.MoodStatModifierProvider"/>.
    /// </summary>
    public MoodStatModifiers StatModifiers { get; set; } = new();
}

/// <summary>Stat modifiers applied while a mood is active.</summary>
public sealed class MoodStatModifiers
{
    /// <summary>Speed multiplier. 1.0 = no effect.</summary>
    public float SpeedMultiplier { get; set; } = 1.0f;

    /// <summary>Skill XP multiplier. 1.0 = no effect.</summary>
    public float SkillXpMultiplier { get; set; } = 1.0f;
}
