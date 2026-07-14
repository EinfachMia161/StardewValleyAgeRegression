using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace AgeRegression.UI;

/// <summary>
/// Authoritative sprite resolution pipeline. Loads and caches sprite sheet
/// textures keyed by their path so that each asset is loaded at most once per
/// session, regardless of how many wardrobe items reference it.
///
/// <para>
/// All wardrobe render paths (world object, inventory, held object, HUD) must
/// resolve textures through this cache rather than calling
/// <see cref="Game1.content"/> directly. Missing assets are cached as
/// <c>null</c> to avoid repeated load attempts every frame.
/// </para>
/// </summary>
public static class SpriteResolver
{
    private static readonly Dictionary<string, Texture2D?> TextureCache =
        new Dictionary<string, Texture2D?>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns the cached texture for <paramref name="spriteSheetPath"/>, loading
    /// and caching it on first use. Returns <c>null</c> if the asset cannot be
    /// loaded.
    /// </summary>
    public static Texture2D? LoadTexture(string spriteSheetPath)
    {
        if (TextureCache.TryGetValue(spriteSheetPath, out var cached))
            return cached;

        Texture2D? texture = null;
        try
        {
            texture = Game1.content.Load<Texture2D>(spriteSheetPath);
        }
        catch
        {
            texture = null;
        }

        TextureCache[spriteSheetPath] = texture;
        return texture;
    }
}
