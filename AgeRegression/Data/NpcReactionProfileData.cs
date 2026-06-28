namespace AgeRegression.Data;

/// <summary>
/// Defines how a specific NPC reacts to the player's regression state.
/// Loaded from <c>assets/data/npc-reactions.json</c>.
/// </summary>
public sealed class NpcReactionProfileData
{
    /// <summary>
    /// The NPC's name as it appears in
    /// <c>Game1.getCharacterFromName</c>.
    /// Example: <c>"Abigail"</c>.
    /// </summary>
    public string NpcName { get; set; } = string.Empty;

    /// <summary>
    /// Personality tags that influence dialogue selection.
    /// Example tags: <c>"caring"</c>, <c>"teasing"</c>,
    /// <c>"neutral"</c>, <c>"embarrassed"</c>, <c>"supportive"</c>.
    /// Multiple tags are allowed.
    /// </summary>
    public List<string> PersonalityTags { get; set; } = new();

    /// <summary>
    /// Whether this NPC can act as a caregiver.
    /// Caregivers have access to additional interaction options.
    /// </summary>
    public bool CanBeCaregiver { get; set; } = false;

    /// <summary>
    /// The dialogue file key used to look up this NPC's dialogue pack.
    /// Defaults to the NPC name if not specified.
    /// </summary>
    public string? DialoguePackKey { get; set; }
}
