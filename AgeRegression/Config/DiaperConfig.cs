namespace AgeRegression.Config;

/// <summary>
/// Configuration for diaper mechanics.
/// </summary>
public sealed class DiaperConfig
{
    /// <summary>Whether the diaper wetness system is active.</summary>
    public bool WetnessEnabled { get; set; } = true;

    /// <summary>Whether optional messing mechanics are enabled.</summary>
    public bool MessingEnabled { get; set; } = false;

    /// <summary>Whether accident mechanics are enabled.</summary>
    public bool AccidentsEnabled { get; set; } = true;

    /// <summary>
    /// Base chance (0.0–1.0) of a wetting accident per wetness tick
    /// at the deepest regression stage with an empty diaper.
    /// Actual chance is scaled by regression depth and diaper fullness.
    /// </summary>
    public float BaseAccidentChance { get; set; } = 0.05f;

    /// <summary>
    /// Multiplier applied to the base accident chance to determine
    /// messing accident chance. Only used when <see cref="MessingEnabled"/>
    /// is true.
    /// Example: 0.3 means messing accidents are 30% as likely as wetting.
    /// </summary>
    public float MessingAccidentChanceMultiplier { get; set; } = 0.3f;

    /// <summary>
    /// How much messing level increases per messing accident (0.0–1.0).
    /// </summary>
    public float MessingIncreasePerAccident { get; set; } = 0.25f;

    /// <summary>
    /// How many in-game minutes between wetness tick evaluations.
    /// Minimum 10. Default is 30.
    /// </summary>
    public int WetnessTickIntervalMinutes { get; set; } = 30;
}
