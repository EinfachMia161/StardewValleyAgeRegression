namespace AgeRegression.State;

/// <summary>
/// Small value object for a normalized need value.
/// </summary>
public sealed class NeedsValue
{
    public float Normalized { get; set; } = 1f;

    public string LastKnownThresholdId { get; set; } = string.Empty;

    public void ApplyDelta(float delta)
    {
        Normalized = Math.Clamp(Normalized - delta, 0f, 1f);
    }

    /// <summary>Returns a shallow copy (all fields are value types or immutable strings).</summary>
    public NeedsValue Clone() => new()
    {
        Normalized           = Normalized,
        LastKnownThresholdId = LastKnownThresholdId
    };
}
