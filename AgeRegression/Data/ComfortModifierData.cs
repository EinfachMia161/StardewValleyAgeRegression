namespace AgeRegression.Data;

/// <summary>
/// Defines a named comfort modifier loaded from
/// <c>assets/data/comfort-modifiers.json</c>.
///
/// <para>
/// Each modifier represents a source that contributes positively or
/// negatively to the player's comfort score. The system evaluates all
/// modifiers each tick and sums the active ones.
/// </para>
/// </summary>
public sealed class ComfortModifierData
{
    /// <summary>
    /// Unique identifier for this modifier source.
    /// Example: <c>"diaper_clean"</c>, <c>"diaper_soaked"</c>,
    /// <c>"pacifier_equipped"</c>, <c>"low_continence"</c>.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description for tooling and debug output.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The comfort delta applied per in-game hour while this modifier
    /// is active. Positive values increase comfort; negative values
    /// decrease it.
    /// </summary>
    public float ValuePerHour { get; set; } = 0f;

    /// <summary>
    /// Flat comfort bonus/penalty applied immediately when this modifier
    /// becomes active (transitions from inactive to active).
    /// 0 means no immediate effect.
    /// </summary>
    public float ImmediateValue { get; set; } = 0f;

    /// <summary>
    /// Conditions that must be satisfied for this modifier to be active.
    /// Uses the same <see cref="DialogueConditions"/> model as dialogue,
    /// so comfort reacts to the same state dimensions as dialogue selection.
    /// </summary>
    public DialogueConditions? Conditions { get; set; }

    /// <summary>
    /// Priority order for display and conflict resolution.
    /// Higher values are evaluated first.
    /// </summary>
    public int Priority { get; set; } = 0;
}
