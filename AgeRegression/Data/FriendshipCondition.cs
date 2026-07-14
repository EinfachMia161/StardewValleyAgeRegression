namespace AgeRegression.Data;

/// <summary>
/// Immutable unlock condition requiring at least the specified friendship
/// points with the named NPC.
/// </summary>
public sealed class FriendshipCondition : IUnlockCondition
{
    /// <summary>NPC whose friendship level is checked.</summary>
    public string NpcName { get; }

    /// <summary>Minimum required friendship points.</summary>
    public int RequiredPoints { get; }

    public FriendshipCondition(string npcName, int requiredPoints)
    {
        NpcName = npcName;
        RequiredPoints = requiredPoints;
    }
}
