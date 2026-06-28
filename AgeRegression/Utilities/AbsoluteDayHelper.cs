namespace AgeRegression.Utilities;

/// <summary>
/// Converts Stardew Valley date components into a single monotonically
/// increasing integer representing the total number of days elapsed
/// since the start of Year 1, Spring, Day 1.
///
/// <para>
/// Formula:
/// <c>((year - 1) * seasonsPerYear * daysPerSeason)
///  + (seasonIndex * daysPerSeason)
///  + (dayOfMonth - 1)</c>
/// </para>
///
/// <para>
/// Day 0 = Year 1, Spring, Day 1.
/// This value is stable across save/load cycles because it is derived
/// entirely from game state that Stardew Valley persists in the save file.
/// </para>
/// </summary>
public static class AbsoluteDayHelper
{
    private const int DaysPerSeason = 28;
    private const int SeasonsPerYear = 4;
    private const int DaysPerYear = DaysPerSeason * SeasonsPerYear; // 112

    /// <summary>
    /// Converts a year, season index (0=spring, 1=summer, 2=fall, 3=winter),
    /// and day of month (1–28) into an absolute day number.
    /// </summary>
    /// <param name="year">Game year, starting at 1.</param>
    /// <param name="seasonIndex">
    /// Season index: 0=spring, 1=summer, 2=fall, 3=winter.
    /// </param>
    /// <param name="dayOfMonth">Day within the season, 1–28.</param>
    /// <returns>Absolute day number, starting at 0.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if any argument is outside its valid range.
    /// </exception>
    public static int ToAbsoluteDay(int year, int seasonIndex, int dayOfMonth)
    {
        if (year < 1)
            throw new ArgumentOutOfRangeException(
                nameof(year), "Year must be >= 1.");
        if (seasonIndex is < 0 or > 3)
            throw new ArgumentOutOfRangeException(
                nameof(seasonIndex), "Season index must be 0–3.");
        if (dayOfMonth is < 1 or > DaysPerSeason)
            throw new ArgumentOutOfRangeException(
                nameof(dayOfMonth),
                $"Day of month must be 1–{DaysPerSeason}.");

        return ((year - 1) * DaysPerYear)
             + (seasonIndex * DaysPerSeason)
             + (dayOfMonth - 1);
    }

    /// <summary>
    /// Returns the absolute day number for the current in-game date.
    /// Safe to call only when a save is loaded
    /// (<c>Context.IsWorldReady</c>).
    /// </summary>
    public static int GetCurrentAbsoluteDay()
    {
        var date = StardewValley.Game1.Date;
        return ToAbsoluteDay(date.Year, date.SeasonIndex, date.DayOfMonth);
    }

    /// <summary>
    /// Returns the number of days elapsed between two absolute day values.
    /// Always returns a non-negative value — returns 0 if
    /// <paramref name="toAbsoluteDay"/> is not after
    /// <paramref name="fromAbsoluteDay"/>.
    /// </summary>
    public static int DaysBetween(int fromAbsoluteDay, int toAbsoluteDay) =>
        Math.Max(0, toAbsoluteDay - fromAbsoluteDay);
}
