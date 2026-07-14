using AgeRegression.Items;
using StardewValley;

namespace AgeRegression.Integrations;

/// <summary>
/// Production implementation of <see cref="IGameStateProvider"/>.
/// Reads game progression state from <see cref="Game1"/>.
/// This is the only class in the domain pipeline that touches Stardew APIs
/// for game-state queries.
/// </summary>
public sealed class StardewGameStateProvider : IGameStateProvider
{
    /// <inheritdoc/>
    public int CurrentYear => Game1.year;

    /// <inheritdoc/>
    public string CurrentSeason => Game1.currentSeason ?? "spring";

    /// <inheritdoc/>
    public int? GetFriendshipPoints(string npcName)
    {
        var friendship = Game1.player?.friendshipData;
        if (friendship is null) return null;
        return friendship.TryGetValue(npcName, out var data) ? data.Points : null;
    }

    /// <inheritdoc/>
    public bool HasReceivedMail(string mailFlag)
    {
        return Game1.player?.mailReceived?.Contains(mailFlag) == true;
    }
}
