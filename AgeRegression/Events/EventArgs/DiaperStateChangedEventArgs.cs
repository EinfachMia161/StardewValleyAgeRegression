using AgeRegression.State;

namespace AgeRegression.Events;

/// <summary>
/// Published when the player's diaper state changes (wetness, soiling,
/// or equipped diaper type).
/// </summary>
public sealed class DiaperStateChangedEventArgs
{
    /// <summary>
    /// A snapshot of the diaper state before the change.
    /// </summary>
    public DiaperState PreviousState { get; }

    /// <summary>
    /// The current diaper state after the change.
    /// </summary>
    public DiaperState NewState { get; }

    /// <summary>
    /// The unique ID of the player whose diaper state changed.
    /// </summary>
    public long PlayerId { get; }

    public DiaperStateChangedEventArgs(
        DiaperState previousState,
        DiaperState newState,
        long playerId)
    {
        PreviousState = previousState;
        NewState      = newState;
        PlayerId      = playerId;
    }
}
