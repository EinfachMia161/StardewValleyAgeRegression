using AgeRegression.Data;
using AgeRegression.Items;
using AgeRegression.UI;
using AgeRegression.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using DataLoader = AgeRegression.Data.DataLoader;

namespace AgeRegression.Patches;

/// <summary>
/// Patches Stardew Valley's Object drawing methods to use the mod's custom
/// sprite sheets (64x64 sprites in a 13-column grid) for diaper and accessory
/// items instead of the default 16x16 object drawing.
///
/// <para>
/// Every render path resolves its sprite through <see cref="SpriteResolver"/>
/// using a <see cref="SpriteReference"/> produced by
/// <see cref="GetSpriteReference"/>. The item's own sprite sheet is tried
/// first, with a fallback to the placeholder and diaper sheets for backwards
/// compatibility.
/// </para>
/// </summary>
internal static class ObjectRenderPatch
{
    private static DataLoader? _dataLoader;
    private static LogHelper? _log;

    /// <summary>
    /// Initialize the patch with required dependencies.
    /// </summary>
    public static void Initialize(DataLoader dataLoader, LogHelper log)
    {
        _dataLoader = dataLoader;
        _log = log;
    }

    // -------------------------------------------------------------------------
    // Patch: Object.draw (world rendering)
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(StardewValley.Object), "draw")]
    private static class ObjectDrawPatch
    {
        private static bool Prefix(
            StardewValley.Object __instance,
            SpriteBatch spriteBatch,
            int x,
            int y,
            float alpha)
        {
            return DrawCustomItem(__instance, spriteBatch, x, y, alpha);
        }
    }

    // -------------------------------------------------------------------------
    // Patch: Object.drawInMenu (inventory rendering)
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(StardewValley.Object), "drawInMenu")]
    private static class ObjectDrawInMenuPatch
    {
        private static bool Prefix(
            StardewValley.Object __instance,
            SpriteBatch spriteBatch,
            Vector2 location,
            float scale)
        {
            if (!TryGetSpriteDraw(__instance, out var texture, out var srcRect))
                return true; // Use default drawing

            // Draw the sprite scaled to fit in inventory slot (64px sprites scaled to 16px)
            spriteBatch.Draw(
                texture,
                location,
                srcRect,
                Color.White,
                0f,
                Vector2.Zero,
                scale * 0.25f, // Scale: 64px -> 16px
                SpriteEffects.None,
                0f);

            return false; // Skip original method
        }
    }

    // -------------------------------------------------------------------------
    // Patch: Object.drawWhenHeld (held item rendering)
    // -------------------------------------------------------------------------

    [HarmonyPatch(typeof(StardewValley.Object), "drawWhenHeld")]
    private static class ObjectDrawWhenHeldPatch
    {
        private static bool Prefix(
            StardewValley.Object __instance,
            SpriteBatch spriteBatch)
        {
            if (!TryGetSpriteDraw(__instance, out var texture, out var srcRect))
                return true; // Use default drawing

            // Draw centered on screen
            var viewport = Game1.graphics.GraphicsDevice.Viewport;
            var destRect = new Rectangle(
                (viewport.Width - HudRenderer.SpriteDim) / 2,
                (viewport.Height - HudRenderer.SpriteDim) / 2,
                HudRenderer.SpriteDim,
                HudRenderer.SpriteDim);

            spriteBatch.Draw(texture, destRect, srcRect, Color.White);
            return false; // Skip original method
        }
    }

    // -------------------------------------------------------------------------
    // Shared resolution pipeline (used by every render path)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Resolves <paramref name="obj"/> to a texture and source rectangle using
    /// the shared <see cref="SpriteResolver"/> pipeline. Returns
    /// <c>false</c> when the object is not a custom wardrobe item or its sheet
    /// cannot be loaded, in which case the original draw should run.
    /// </summary>
    private static bool TryGetSpriteDraw(
        StardewValley.Object obj,
        out Texture2D texture,
        out Rectangle srcRect)
    {
        texture = null!;
        srcRect = default;

        var sprite = GetSpriteReference(obj);
        if (sprite is null)
            return false;

        var tex = ResolveTexture(sprite);
        if (tex is null)
            return false;

        texture = tex;
        srcRect = sprite.GetSourceRectangle(HudRenderer.SpriteColumns, HudRenderer.SpriteDim);
        return true;
    }

    private static bool DrawCustomItem(
        StardewValley.Object obj,
        SpriteBatch spriteBatch,
        int x,
        int y,
        float alpha)
    {
        if (!TryGetSpriteDraw(obj, out var texture, out var srcRect))
            return true; // Use default drawing

        var destRect = new Rectangle(
            x - HudRenderer.SpriteDim / 2 + Game1.tileSize / 2,
            y - HudRenderer.SpriteDim / 2 + Game1.tileSize / 2,
            HudRenderer.SpriteDim,
            HudRenderer.SpriteDim);

        spriteBatch.Draw(texture, destRect, srcRect, Color.White * alpha);
        return false; // Skip original method
    }

    /// <summary>
    /// Resolves a custom wardrobe item to its <see cref="SpriteReference"/>, or
    /// <c>null</c> if the object is not a mod diaper/accessory. Add new
    /// wardrobe categories here by returning a <see cref="SpriteReference"/>
    /// from the item's data.
    /// </summary>
    private static SpriteReference? GetSpriteReference(StardewValley.Object obj)
    {
        // Check by ID prefix first (for items not yet equipped)
        if (ItemIds.TryParseDiaperTypeId(obj.QualifiedItemId) is { } diaperId)
        {
            var diaperType = _dataLoader?.GetDiaperType(diaperId);
            if (diaperType is null)
                return null;
            return new SpriteReference(diaperType.SpriteSheet, diaperType.SpriteIndex);
        }

        if (ItemIds.TryParseAccessoryTypeId(obj.QualifiedItemId) is { } accessoryId)
        {
            var accessory = _dataLoader?.GetWardrobeItem(accessoryId);
            if (accessory is null)
                return null;
            return new SpriteReference(accessory.SpriteSheet, accessory.SpriteIndex);
        }

        return null;
    }

    /// <summary>
    /// Loads the item's own sprite sheet, falling back to the placeholder and
    /// diaper sheets for backwards compatibility with items whose sheet is
    /// missing. Textures are cached by <see cref="SpriteResolver"/>.
    /// </summary>
    private static Texture2D? ResolveTexture(SpriteReference sprite)
    {
        return SpriteResolver.LoadTexture(sprite.SpriteSheet)
            ?? SpriteResolver.LoadTexture(HudRenderer.PlaceholderSpritesPath)
            ?? SpriteResolver.LoadTexture("assets/sprites/diapers");
    }
}
