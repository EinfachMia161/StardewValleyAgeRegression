using AgeRegression.Data;

namespace AgeRegression.Events;

/// <summary>
/// Published when the player's regression stage changes.
/// </summary>
public sealed class RegressionChangedEventArgs
{
    /// <summary>
    /// The stage the player was in before the change.
    /// </summary>
    public RegressionStageData PreviousStage { get; }

    /// <summary>
    /// The stage the player has transitioned to.
    /// </summary>
    public RegressionStageData NewStage { get; }

    /// <summary>
    /// The unique ID of the player whose stage changed.
    /// </summary>
    public long PlayerId { get; }

    public RegressionChangedEventArgs(
        RegressionStageData previousStage,
        RegressionStageData newStage,
        long playerId)
    {
        PreviousStage = previousStage;
        NewStage      = newStage;
        PlayerId      = playerId;
    }
}
