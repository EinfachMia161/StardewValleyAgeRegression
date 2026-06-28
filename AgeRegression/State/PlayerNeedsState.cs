namespace AgeRegression.State;

/// <summary>
/// Container for the player's needs values.
/// </summary>
public sealed class PlayerNeedsState
{
    public ContinenceState Continence { get; set; } = new();

    public NeedsValue Hunger { get; set; } = new();

    public NeedsValue Thirst { get; set; } = new();
}
