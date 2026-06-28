namespace AgeRegression.Events;

/// <summary>
/// Published when the player's mood level changes.
/// </summary>
public sealed class MoodChangedEventArgs
{
    /// <summary>The mood ID before the change.</summary>
    public string PreviousMoodId { get; }

    /// <summary>The mood ID after the change.</summary>
    public string NewMoodId { get; }

    /// <summary>
    /// The unique ID of the player whose mood changed.
    /// </summary>
    public long PlayerId { get; }

    public MoodChangedEventArgs(
        string previousMoodId,
        string newMoodId,
        long playerId)
    {
        PreviousMoodId = previousMoodId;
        NewMoodId      = newMoodId;
        PlayerId       = playerId;
    }
}
