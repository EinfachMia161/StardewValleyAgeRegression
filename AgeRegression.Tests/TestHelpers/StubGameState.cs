using AgeRegression.Items;

namespace AgeRegression.Tests;

internal sealed class StubGameState(
    int year = 1,
    string season = "spring",
    Dictionary<string, int>? friendship = null,
    HashSet<string>? mailFlags = null)
    : IGameStateProvider
{
    public int CurrentYear { get; } = year;
    public string CurrentSeason { get; } = season;

    public int? GetFriendshipPoints(string npcName) =>
        friendship is not null && friendship.TryGetValue(npcName, out var pts)
            ? pts
            : 0;

    public bool HasReceivedMail(string mailFlag) =>
        mailFlags is not null && mailFlags.Contains(mailFlag);
}
