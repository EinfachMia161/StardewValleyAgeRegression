namespace AgeRegression.Events;

/// <summary>
/// Published when a needs value crosses into a new threshold band.
/// Subscribers (e.g. <see cref="Systems.DiaperSystem"/>) react to
/// specific threshold IDs without being coupled to the needs systems.
/// </summary>
public sealed class NeedThresholdCrossedEventArgs
{
    /// <summary>
    /// Which need crossed a threshold.
    /// Values: <c>"continence"</c>, <c>"hunger"</c>, <c>"thirst"</c>.
    /// </summary>
    public string NeedId { get; }

    /// <summary>
    /// The threshold band the value was in before the crossing.
    /// </summary>
    public string PreviousThresholdId { get; }

    /// <summary>The threshold band the value has entered.</summary>
    public string NewThresholdId { get; }

    /// <summary>The current normalized value (0.0–1.0).</summary>
    public float CurrentNormalized { get; }

    /// <summary>
    /// Whether the crossing was a deterioration (value decreased into
    /// a worse band) or an improvement (value increased into a better
    /// band).
    /// </summary>
    public bool IsDeteriorating { get; }

    /// <summary>
    /// The unique ID of the player whose need crossed a threshold.
    /// </summary>
    public long PlayerId { get; }

    public NeedThresholdCrossedEventArgs(
        string needId,
        string previousThresholdId,
        string newThresholdId,
        float currentNormalized,
        bool isDeteriorating,
        long playerId)
    {
        NeedId              = needId;
        PreviousThresholdId = previousThresholdId;
        NewThresholdId      = newThresholdId;
        CurrentNormalized   = currentNormalized;
        IsDeteriorating     = isDeteriorating;
        PlayerId            = playerId;
    }
}
