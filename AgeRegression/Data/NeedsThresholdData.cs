namespace AgeRegression.Data;

/// <summary>
/// Defines a named threshold band for a needs value.
/// Thresholds are ordered by <see cref="MinNormalized"/> ascending.
/// When a needs value crosses into a new band, an event is fired.
/// </summary>
public sealed class NeedsThresholdData
{
    /// <summary>
    /// Unique identifier for this threshold band.
    /// Example: <c>"comfortable"</c>, <c>"warning"</c>,
    /// <c>"struggling"</c>, <c>"loss_of_control"</c>.
    /// Never referenced by string constant in C# — always carried as data.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The normalized value (0.0–1.0) at which this band begins.
    /// The band extends from this value up to the next threshold's
    /// <see cref="MinNormalized"/> (or 1.0 for the highest band).
    /// </summary>
    public float MinNormalized { get; set; } = 0f;

    /// <summary>
    /// Sort order. Lower values represent worse states (more urgent).
    /// </summary>
    public int Order { get; set; } = 0;
}

/// <summary>
/// Groups threshold definitions for a specific need type.
/// </summary>
public sealed class NeedsThresholdSetData
{
    /// <summary>
    /// The need this threshold set applies to.
    /// Values: <c>"continence"</c>, <c>"hunger"</c>, <c>"thirst"</c>.
    /// </summary>
    public string NeedId { get; set; } = string.Empty;

    /// <summary>
    /// Threshold bands, ordered by
    /// <see cref="NeedsThresholdData.MinNormalized"/>.
    /// </summary>
    public List<NeedsThresholdData> Thresholds { get; set; } = new();
}
