namespace AgeRegression.State;

/// <summary>
/// Tracks player care history for roleplay and dialogue context.
/// NOT an achievement or progression system.
/// </summary>
public sealed class CareState
{
    /// <summary>
    /// The absolute day number of the last diaper change.
    /// Used for dialogue context like "DaysSinceLastDiaperChange".
    /// </summary>
    public int LastDiaperChangeAbsoluteDay { get; set; } = 0;

    /// <summary>
    /// Number of care actions performed today (resets at day start).
    /// </summary>
    public int CareActionsToday { get; set; }

    /// <summary>
    /// The ID of the last care action performed.
    /// </summary>
    public string LastCareActionId { get; set; } = string.Empty;

    /// <summary>
    /// The absolute day number when CareActionsToday was reset.
    /// Used to detect day boundaries.
    /// </summary>
    public int LastCareDayAbsolute { get; set; }
}