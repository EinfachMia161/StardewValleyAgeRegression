namespace AgeRegression.Config;

/// <summary>
/// Configuration for the comfort system.
/// </summary>
public sealed class ComfortConfig
{
    /// <summary>Whether the comfort system is active.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum comfort score. Comfort is tracked as a value from 0
    /// to this maximum.
    /// </summary>
    public float MaxComfort { get; set; } = 100f;

    /// <summary>
    /// Passive comfort decay per in-game hour while regressed.
    /// Set to 0 to disable passive decay.
    /// This value is superseded by the data-driven comfort modifier system
    /// (<c>assets/data/comfort-modifiers.json</c>) which handles per-hour
    /// values. This field is retained for future use as a global scalar.
    /// </summary>
    public float PassiveDecayPerHour { get; set; } = 2f;
}
