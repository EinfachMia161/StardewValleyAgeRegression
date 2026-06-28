namespace AgeRegression.Utilities;

/// <summary>
/// Helpers for working with Stardew Valley's integer time format.
/// Times are represented as integers like 600 (6:00 AM), 1200 (12:00 PM),
/// 2600 (2:00 AM next day).
/// </summary>
public static class GameTimeHelper
{
    /// <summary>
    /// Returns the number of in-game minutes elapsed between two time values.
    /// Handles the case where <paramref name="toTime"/> is on the next day
    /// (i.e., past midnight, represented as values above 2400).
    /// Returns 0 if <paramref name="toTime"/> is not after
    /// <paramref name="fromTime"/>.
    /// </summary>
    public static int MinutesBetween(int fromTime, int toTime)
    {
        if (toTime <= fromTime) return 0;
        return ToMinutes(toTime) - ToMinutes(fromTime);
    }

    /// <summary>
    /// Converts a Stardew time integer to total minutes since midnight.
    /// Example: 630 ? 6*60 + 30 = 390 minutes.
    /// </summary>
    public static int ToMinutes(int stardewTime)
    {
        var hours   = stardewTime / 100;
        var minutes = stardewTime % 100;
        return (hours * 60) + minutes;
    }
}
