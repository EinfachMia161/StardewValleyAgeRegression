using AgeRegression.Items;

namespace AgeRegression.Tests;

/// <summary>
/// <see cref="IGameStateProvider"/> that reports maximum progression so
/// every item unlock requirement is satisfied. Intended for tests that
/// exercise acquisition/stock logic without unlock gating.
/// </summary>
internal sealed class AlwaysUnlockedGameState : IGameStateProvider
{
    public int CurrentYear => int.MaxValue;
    public string CurrentSeason => "spring";

    public int? GetFriendshipPoints(string npcName) => int.MaxValue;

    public bool HasReceivedMail(string mailFlag) => true;
}
