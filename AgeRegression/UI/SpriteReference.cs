using Microsoft.Xna.Framework;

namespace AgeRegression.UI;

/// <summary>
/// Immutable reference to a single sprite within a sprite sheet.
///
/// <para>
/// Encapsulates the sprite sheet path and the zero-based sprite index so that
/// every render path (world, inventory, held, HUD) resolves sprites through one
/// pipeline instead of recomputing source rectangles inline. The source
/// rectangle is derived on demand from the shared grid constants in
/// <see cref="HudRenderer"/>.
/// </para>
/// </summary>
public sealed record SpriteReference
{
    /// <summary>
    /// Creates a reference to the sprite at <paramref name="spriteIndex"/> on
    /// the sheet at <paramref name="spriteSheet"/>.
    /// </summary>
    /// <param name="spriteSheet">Sprite sheet path relative to the mod root (no extension).</param>
    /// <param name="spriteIndex">Zero-based sprite index within the sheet.</param>
    public SpriteReference(string spriteSheet, int spriteIndex)
    {
        SpriteSheet = spriteSheet;
        SpriteIndex = spriteIndex;
    }

    /// <summary>Sprite sheet path relative to the mod root (no extension).</summary>
    public string SpriteSheet { get; init; }

    /// <summary>Zero-based sprite index within the sheet.</summary>
    public int SpriteIndex { get; init; }

    /// <summary>
    /// Computes the source rectangle for this sprite assuming a uniform grid of
    /// <paramref name="spriteDim"/>-pixel squares laid out in
    /// <paramref name="columns"/> per row.
    /// </summary>
    public Rectangle GetSourceRectangle(int columns, int spriteDim)
    {
        return new Rectangle(
            (SpriteIndex % columns) * spriteDim,
            (SpriteIndex / columns) * spriteDim,
            spriteDim,
            spriteDim);
    }
}
