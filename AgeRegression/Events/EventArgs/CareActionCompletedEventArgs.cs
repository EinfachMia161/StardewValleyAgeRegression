namespace AgeRegression.Events;

/// <summary>
/// Published when a care action is successfully completed.
/// Used by ComfortSystem and future dialogue systems.
/// </summary>
public sealed class CareActionCompletedEventArgs
{
    /// <summary>The ID of the care action that was performed.</summary>
    public string CareActionId { get; }

    /// <summary>
    /// The unique ID of the player who received care.
    /// </summary>
    public long PlayerId { get; }

    /// <summary>
    /// The absolute day number when the action occurred.
    /// Used to reset daily counters.
    /// </summary>
    public int AbsoluteDay { get; }

    /// <summary>
    /// The current location name where the action occurred (optional).
    /// </summary>
    public string? LocationName { get; }

    /// <summary>Additional context for the care action (optional).</summary>
    public string? Context { get; }

    public CareActionCompletedEventArgs(
        string careActionId,
        long playerId,
        int absoluteDay,
        string? locationName = null,
        string? context = null)
    {
        CareActionId = careActionId;
        PlayerId = playerId;
        AbsoluteDay = absoluteDay;
        LocationName = locationName;
        Context = context;
    }
}