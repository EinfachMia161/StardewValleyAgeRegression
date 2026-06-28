namespace AgeRegression.State;

/// <summary>
/// State for the continence need.
/// </summary>
public sealed class ContinenceState
{
    public NeedsValue Value { get; set; } = new();

    public float StressModifier { get; set; }

    public bool LostControlToday { get; set; }
}
