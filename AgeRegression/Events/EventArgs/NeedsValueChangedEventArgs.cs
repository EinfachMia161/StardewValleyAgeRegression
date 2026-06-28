namespace AgeRegression.Events;

/// <summary>
/// Published when any needs value changes by a meaningful amount.
/// Used by UI and comfort systems to react to needs changes.
/// </summary>
public sealed class NeedsValueChangedEventArgs
{
    /// <summary>
    /// Which need changed.
    /// Values: <c>"continence"</c>, <c>"hunger"</c>, <c>"thirst"</c>.
    /// </summary>
    public string NeedId { get; }

    /// <summary>Normalized value before the change (0.0–1.0).</summary>
    public float PreviousNormalized { get; }

    /// <summary>Normalized value after the change (0.0–1.0).</summary>
    public float NewNormalized { get; }

    /// <summary>
    /// The unique ID of the player whose need changed.
    /// </summary>
    public long PlayerId { get; }

    public NeedsValueChangedEventArgs(
        string needId,
        float previousNormalized,
        float newNormalized,
        long playerId)
    {
        NeedId             = needId;
        PreviousNormalized = previousNormalized;
        NewNormalized      = newNormalized;
        PlayerId           = playerId;
    }
}
