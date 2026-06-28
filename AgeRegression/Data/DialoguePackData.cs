namespace AgeRegression.Data;

/// <summary>
/// A collection of dialogue entries for a specific NPC or context,
/// loaded from a single JSON file under <c>assets/dialogue/</c>.
/// </summary>
public sealed class DialoguePackData
{
    /// <summary>
    /// Identifier for this pack. For NPC packs this matches the NPC name.
    /// For generic packs this is a descriptive key like
    /// <c>"generic_fallback"</c>.
    /// </summary>
    public string PackId { get; set; } = string.Empty;

    /// <summary>
    /// Optional display name for tooling and debugging.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// The dialogue entries in this pack.
    /// </summary>
    public List<DialogueEntryData> Entries { get; set; } = new();
}
