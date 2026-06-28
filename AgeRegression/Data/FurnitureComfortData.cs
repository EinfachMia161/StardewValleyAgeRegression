namespace AgeRegression.Data;

/// <summary>
/// Maps furniture item IDs to the comfort modifier they activate
/// when the player is nearby.
/// Loaded from <c>assets/data/furniture-comfort.json</c>.
///
/// <para>
/// This indirection allows the same comfort modifier to be shared
/// across multiple furniture items, and allows furniture comfort
/// effects to be adjusted without changing furniture definitions.
/// </para>
/// </summary>
public sealed class FurnitureComfortData
{
    /// <summary>
    /// The furniture item ID (matches <see cref="FurnitureItemData.Id"/>).
    /// </summary>
    public string FurnitureId { get; set; } = string.Empty;

    /// <summary>
    /// The comfort modifier ID to activate when the player is nearby.
    /// Must match an entry in <c>assets/data/comfort-modifiers.json</c>.
    /// </summary>
    public string ComfortModifierId { get; set; } = string.Empty;

    /// <summary>Proximity radius in tiles.</summary>
    public int ProximityTiles { get; set; } = 3;
}
