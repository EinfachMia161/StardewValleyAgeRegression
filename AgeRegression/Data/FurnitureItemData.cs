namespace AgeRegression.Data;

/// <summary>
/// Defines a custom furniture item registered in <c>Data/Furniture</c>.
/// Loaded from <c>assets/data/furniture-items.json</c>.
/// </summary>
public sealed class FurnitureItemData
{
    /// <summary>
    /// Unique identifier. Used to construct the furniture ID:
    /// <c>mia.AgeRegression_{Id}</c>.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Display name shown in inventory and shop.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Item description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Furniture type as defined by SV's Data/Furniture format.
    /// Common values: <c>"other"</c>, <c>"bed"</c>, <c>"dresser"</c>,
    /// <c>"painting"</c>, <c>"lamp"</c>, <c>"decor"</c>.
    /// </summary>
    public string FurnitureType { get; set; } = "other";

    /// <summary>Tile width in the world.</summary>
    public int TileWidth { get; set; } = 2;

    /// <summary>Tile height in the world.</summary>
    public int TileHeight { get; set; } = 2;

    /// <summary>Bounding box height for collision.</summary>
    public int BoundingBoxHeight { get; set; } = 1;

    /// <summary>Sprite index in the furniture sprite sheet.</summary>
    public int SpriteIndex { get; set; } = 0;

    /// <summary>Price in gold.</summary>
    public int Price { get; set; } = 500;

    /// <summary>
    /// Whether this furniture provides a comfort effect when the player
    /// is nearby. Links to a <see cref="FurnitureComfortData"/> entry.
    /// </summary>
    public bool ProvidesComfortEffect { get; set; } = false;

    /// <summary>
    /// The comfort modifier ID activated when the player is near this
    /// furniture. Must match an entry in
    /// <c>assets/data/comfort-modifiers.json</c>.
    /// </summary>
    public string? ComfortModifierId { get; set; }

    /// <summary>
    /// Proximity radius in tiles for comfort effect activation.
    /// </summary>
    public int ComfortProximityTiles { get; set; } = 3;
}
