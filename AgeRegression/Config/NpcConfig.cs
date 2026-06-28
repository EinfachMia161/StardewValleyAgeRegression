namespace AgeRegression.Config;

/// <summary>
/// Configuration for NPC and spouse interaction systems.
/// </summary>
public sealed class NpcConfig
{
    /// <summary>Whether NPCs react to the player's regression state.</summary>
    public bool NpcReactionsEnabled { get; set; } = true;

    /// <summary>Whether spouse-specific extended interactions are enabled.</summary>
    public bool SpouseInteractionsEnabled { get; set; } = true;

    /// <summary>
    /// Minimum friendship hearts required before an NPC will acknowledge
    /// the player's regression state. Range 0–14.
    /// </summary>
    public int MinFriendshipForReaction { get; set; } = 2;

    /// <summary>
    /// Cooldown in in-game days before the same NPC will repeat
    /// a regression-related dialogue line.
    /// </summary>
    public int DialogueCooldownDays { get; set; } = 1;
}
