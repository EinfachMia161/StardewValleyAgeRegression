namespace AgeRegression.Items;

/// <summary>
/// Provides game progression state to domain services.
/// Implemented by a Stardew-aware adapter in production;
/// replaced by a stub in tests.
/// </summary>
/// <remarks>
/// This interface is intentionally narrow — only expose values that
/// domain services actually need. Add members here as new systems
/// require them (crafting, quests, mail, etc.).
/// No Stardew types may appear in this interface.
/// </remarks>
public interface IGameStateProvider
{
    /// <summary>Current in-game year (1-based).</summary>
    int CurrentYear { get; }

    /// <summary>
    /// Current season name, lower-case.
    /// One of: <c>spring</c>, <c>summer</c>, <c>fall</c>, <c>winter</c>.
    /// </summary>
    string CurrentSeason { get; }

    /// <summary>
    /// Returns the player's current friendship points with the named NPC,
    /// or <c>null</c> if the NPC is unknown or not yet met.
    /// </summary>
    int? GetFriendshipPoints(string npcName);

    /// <summary>
    /// Returns <c>true</c> when the player has received the specified mail flag.
    /// </summary>
    bool HasReceivedMail(string mailFlag);
}
