namespace AgeRegression.Config;

/// <summary>
/// Configuration for the regression stage system.
/// </summary>
public sealed class RegressionConfig
{
    /// <summary>
    /// Whether regression stages affect player stats (speed, energy, etc.).
    /// </summary>
    public bool StatModifiersEnabled { get; set; } = true;

    /// <summary>
    /// Whether the player can manually change their regression stage
    /// through the in-game menu.
    /// </summary>
    public bool AllowManualRegression { get; set; } = true;

    /// <summary>
    /// The stage ID the player starts at when a new save is created.
    /// Must match an ID defined in <c>assets/data/regression-stages.json</c>.
    /// </summary>
    public string DefaultStageId { get; set; } = "none";
}
