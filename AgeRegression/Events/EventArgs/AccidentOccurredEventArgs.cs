namespace AgeRegression.Events;

/// <summary>
/// Published when an accident occurs.
/// </summary>
public sealed class AccidentOccurredEventArgs
{
    /// <summary>The type of accident that occurred.</summary>
    public AccidentType AccidentType { get; }

    /// <summary>
    /// The unique ID of the player who had the accident.
    /// </summary>
    public long PlayerId { get; }

    /// <summary>
    /// Whether the player was wearing a diaper when the accident
    /// occurred.
    /// </summary>
    public bool WasWearingDiaper { get; }

    public AccidentOccurredEventArgs(
        AccidentType accidentType,
        long playerId,
        bool wasWearingDiaper)
    {
        AccidentType    = accidentType;
        PlayerId        = playerId;
        WasWearingDiaper = wasWearingDiaper;
    }
}

/// <summary>The type of accident that occurred.</summary>
public enum AccidentType
{
    /// <summary>A wetting accident.</summary>
    Wetting,

    /// <summary>
    /// A messing accident (requires messing mechanics to be enabled).
    /// </summary>
    Messing
}
