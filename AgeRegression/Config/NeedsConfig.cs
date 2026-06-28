namespace AgeRegression.Config;

/// <summary>
/// Configuration for the needs systems (continence, hunger, thirst).
/// </summary>
public sealed class NeedsConfig
{
    /// <summary>Whether the needs systems are active.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Continence system configuration.</summary>
    public ContinenceConfig Continence { get; set; } = new();

    /// <summary>Hunger system configuration.</summary>
    public HungerConfig Hunger { get; set; } = new();

    /// <summary>Thirst system configuration.</summary>
    public ThirstConfig Thirst { get; set; } = new();
}

/// <summary>Configuration for the continence system.</summary>
public sealed class ContinenceConfig
{
    /// <summary>Whether the continence system is active.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Base drain per in-game hour at the baseline regression stage.
    /// Expressed as a normalized fraction (0.0–1.0) of maximum continence.
    /// Example: 0.04 means the player loses 4% continence per hour at baseline.
    /// </summary>
    public float BaseDrainPerHour { get; set; } = 0.04f;

    /// <summary>
    /// Multiplier applied to drain rate at the deepest regression stage.
    /// Actual drain scales linearly between 1.0 and this value based on
    /// stage depth.
    /// </summary>
    public float MaxRegressionDrainMultiplier { get; set; } = 3.0f;

    /// <summary>
    /// How much the stress modifier (from comfort system) amplifies drain.
    /// Drain multiplier = 1.0 + (stressModifier * StressInfluence).
    /// </summary>
    public float StressInfluence { get; set; } = 0.5f;

    /// <summary>
    /// Recovery rate per in-game hour when the player is not regressed.
    /// Expressed as a normalized fraction.
    /// </summary>
    public float RecoveryPerHour { get; set; } = 0.1f;

    /// <summary>
    /// How many in-game minutes between continence tick evaluations.
    /// Minimum 10.
    /// </summary>
    public int TickIntervalMinutes { get; set; } = 20;
}

/// <summary>Configuration for the hunger system.</summary>
public sealed class HungerConfig
{
    /// <summary>Whether the hunger system is active.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Drain per in-game hour as a normalized fraction (0.0–1.0).
    /// Example: 0.025 means the player loses 2.5% hunger per hour.
    /// </summary>
    public float DrainPerHour { get; set; } = 0.025f;

    /// <summary>
    /// How many in-game minutes between hunger tick evaluations.
    /// </summary>
    public int TickIntervalMinutes { get; set; } = 30;
}

/// <summary>Configuration for the thirst system.</summary>
public sealed class ThirstConfig
{
    /// <summary>Whether the thirst system is active.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Drain per in-game hour as a normalized fraction (0.0–1.0).
    /// Thirst drains slightly faster than hunger by default.
    /// </summary>
    public float DrainPerHour { get; set; } = 0.035f;

    /// <summary>
    /// How many in-game minutes between thirst tick evaluations.
    /// </summary>
    public int TickIntervalMinutes { get; set; } = 30;
}
