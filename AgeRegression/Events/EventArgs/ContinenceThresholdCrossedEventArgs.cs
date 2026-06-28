namespace AgeRegression.Events;

/// <summary>
/// Published specifically when the continence value crosses a threshold.
/// Carries additional continence-specific context beyond the generic
/// <see cref="NeedThresholdCrossedEventArgs"/>.
/// </summary>
public sealed class ContinenceThresholdCrossedEventArgs
{
    /// <summary>
    /// The threshold band the value was in before the crossing.
    /// </summary>
    public string PreviousThresholdId { get; }

    /// <summary>The threshold band the value has entered.</summary>
    public string NewThresholdId { get; }

    /// <summary>
    /// The current normalized continence value (0.0–1.0).
    /// </summary>
    public float CurrentNormalized { get; }

    /// <summary>
    /// Whether this crossing represents a loss of control (i.e. the
    /// new threshold is the lowest defined band).
    /// </summary>
    public bool IsLossOfControl { get; }

    /// <summary>
    /// Whether the player is currently wearing a diaper.
    /// </summary>
    public bool IsWearingDiaper { get; }

    /// <summary>The unique ID of the player.</summary>
    public long PlayerId { get; }

    public ContinenceThresholdCrossedEventArgs(
        string previousThresholdId,
        string newThresholdId,
        float currentNormalized,
        bool isLossOfControl,
        bool isWearingDiaper,
        long playerId)
    {
        PreviousThresholdId = previousThresholdId;
        NewThresholdId      = newThresholdId;
        CurrentNormalized   = currentNormalized;
        IsLossOfControl     = isLossOfControl;
        IsWearingDiaper     = isWearingDiaper;
        PlayerId            = playerId;
    }
}
