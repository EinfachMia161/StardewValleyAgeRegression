namespace AgeRegression.Events;

/// <summary>
/// Published when the player's comfort score changes.
/// </summary>
public sealed class ComfortChangedEventArgs
{
    /// <summary>Comfort value before the change.</summary>
    public float PreviousComfort { get; }

    /// <summary>Comfort value after the change.</summary>
    public float NewComfort { get; }

    /// <summary>
    /// The unique ID of the player whose comfort changed.
    /// </summary>
    public long PlayerId { get; }

    /// <summary>
    /// A short description of what caused the comfort change, for
    /// debugging. Example: <c>"diaper_saturated"</c>,
    /// <c>"pacifier_equipped"</c>.
    /// </summary>
    public string Reason { get; }

    public ComfortChangedEventArgs(
        float previousComfort,
        float newComfort,
        long playerId,
        string reason)
    {
        PreviousComfort = previousComfort;
        NewComfort      = newComfort;
        PlayerId        = playerId;
        Reason          = reason;
    }
}
