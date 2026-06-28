using StardewValley;

namespace AgeRegression.Utilities;

/// <summary>
/// Safe accessors for <see cref="Game1"/> state.
/// Centralizes null checks and context guards so callers do not need
/// to repeat them throughout the codebase.
/// </summary>
public static class GameHelper
{
    /// <summary>
    /// Returns the local player, or <c>null</c> if no save is loaded.
    /// </summary>
    public static Farmer? GetLocalPlayer() =>
        StardewModdingAPI.Context.IsWorldReady ? Game1.player : null;

    /// <summary>
    /// Returns <c>true</c> if a save is loaded and the game world is ready.
    /// </summary>
    public static bool IsWorldReady() => StardewModdingAPI.Context.IsWorldReady;

    /// <summary>
    /// Returns the current in-game time as an integer
    /// (e.g. 630, 1200, 2600).
    /// Returns 0 if no save is loaded.
    /// </summary>
    public static int GetCurrentTime() =>
        StardewModdingAPI.Context.IsWorldReady ? Game1.timeOfDay : 0;

    /// <summary>
    /// Returns the current season name in lowercase
    /// (spring, summer, fall, winter).
    /// Returns an empty string if no save is loaded.
    /// </summary>
    public static string GetCurrentSeason() =>
        StardewModdingAPI.Context.IsWorldReady ? Game1.currentSeason : string.Empty;

    /// <summary>
    /// Returns the current day of month (1–28).
    /// Returns 0 if no save is loaded.
    /// </summary>
    public static int GetCurrentDay() =>
        StardewModdingAPI.Context.IsWorldReady ? Game1.dayOfMonth : 0;

    /// <summary>
    /// Returns <c>true</c> if it is currently raining.
    /// </summary>
    public static bool IsRaining() =>
        StardewModdingAPI.Context.IsWorldReady && Game1.isRaining;

    /// <summary>
    /// Returns the name of the location the local player is currently in,
    /// or an empty string if unavailable.
    /// </summary>
    public static string GetCurrentLocationName() =>
        StardewModdingAPI.Context.IsWorldReady
            ? Game1.currentLocation?.Name ?? string.Empty
            : string.Empty;
}
