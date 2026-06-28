using AgeRegression.Config;
using AgeRegression.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace AgeRegression.UI;

/// <summary>
/// Renders the status HUD overlay using Stardew Valley's SpriteBatch.
/// Does not allocate per-frame and respects all configuration toggles.
/// </summary>
public static class HudRenderer
{
    private const int LineHeight = 12;
    private const int TitleSize  = 20;
    private const int BodySize   = 14;

    /// <summary>
    /// Renders the HUD if <paramref name="hud"/> says it should be visible.
    /// Call from a render hook (e.g. <c>GameLoop.Rendered</c>).
    /// </summary>
    public static void Render(StatusHud hud, SpriteBatch spriteBatch)
    {
        if (hud is null)
            return;

        if (!hud.ShouldRender())
            return;

        var data  = hud.GetDisplayData();
        var cfg   = hud.GetConfig();

        var x = cfg.PositionX;
        var y = cfg.PositionY;
        var scale = cfg.Scale;

        // Background panel
        RenderBackground(spriteBatch, x, y, data, cfg, scale);

        // Title
        var titleY = y + 8 * scale;
        DrawText(spriteBatch, "Age Regression", x + 8 * scale, titleY, Color.White, TitleSize * scale);

        // Separator line
        var separatorY = titleY + TitleSize * scale + 4 * scale;
        DrawSeparator(spriteBatch, x + 8 * scale, separatorY, 140 * scale);

        // Content
        var bodyY = separatorY + 6 * scale;
        bodyY = RenderSection(spriteBatch, "Regression", x + 8 * scale, bodyY, data, cfg.ShowRegressionInfo, scale);
        bodyY = RenderSection(spriteBatch, "Diaper", x + 8 * scale, bodyY, data, cfg.ShowDiaperInfo, scale);
        bodyY = RenderSection(spriteBatch, "Status", x + 8 * scale, bodyY, data, cfg.ShowNeedsInfo, scale);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static float RenderSection(
        SpriteBatch sb,
        string sectionName,
        float x,
        float y,
        HudDisplayData data,
        bool enabled,
        float scale)
    {
        if (!enabled)
            return y;

        // Section label
        DrawText(sb, sectionName, x, y, Color.LightGray, BodySize * scale);
        y += LineHeight * scale + 2 * scale;

        // Section content
        y = RenderLines(sb, x, y, GetLinesForSection(sectionName, data), scale);

        return y + 4 * scale;
    }

    private static float RenderLines(
        SpriteBatch sb,
        float x,
        float y,
        IEnumerable<string> lines,
        float scale)
    {
        foreach (var line in lines)
        {
            DrawText(sb, line, x, y, Color.White, BodySize * scale);
            y += LineHeight * scale;
        }

        return y;
    }

    private static IEnumerable<string> GetLinesForSection(string section, HudDisplayData data)
    {
        return section switch
        {
            "Regression" => GetRegressionLines(data),
            "Diaper"     => GetDiaperLines(data),
            "Status"     => GetStatusLines(data),
            _            => Array.Empty<string>()
        };
    }

    private static IEnumerable<string> GetRegressionLines(HudDisplayData data)
    {
        yield return $"Stage: {data.StageName}";

        if (data.ProgressPercent.HasValue)
            yield return $"Progress: {data.ProgressPercent.Value:F0}%";
    }

    private static IEnumerable<string> GetDiaperLines(HudDisplayData data)
    {
        if (data.DiaperCondition == "none")
        {
            yield return "Diaper: None";
            yield break;
        }

        yield return $"Diaper: {Capitalize(data.DiaperCondition)}";
        yield return $"Wetness: {data.WetnessPercent:F0}%";
        yield return $"Messing: {data.MessingPercent:F0}%";
    }

    private static IEnumerable<string> GetStatusLines(HudDisplayData data)
    {
        yield return $"Comfort: {data.Comfort:F0}";
        yield return $"Mood: {data.Mood}";
        yield return $"Continence: {data.Continence:F0}%";
    }

    private static void RenderBackground(
        SpriteBatch sb,
        float x,
        float y,
        HudDisplayData data,
        HudConfig cfg,
        float scale)
    {
        // Estimate panel height
        var lineCount = 2; // title + separator
        if (cfg.ShowRegressionInfo) lineCount += data.ProgressPercent.HasValue ? 3 : 2;
        if (cfg.ShowDiaperInfo)
        {
            lineCount += data.DiaperCondition == "none" ? 2 : 4;
        }
        if (cfg.ShowNeedsInfo) lineCount += 4;

        var panelWidth  = 160 * scale;
        var panelHeight = (lineCount * LineHeight + 16) * scale;

        // Semi-transparent dark background
        sb.Draw(
            Game1.staminaRect,
            new Rectangle((int)x, (int)y, (int)panelWidth, (int)panelHeight),
            new Color(0, 0, 0, 180));

        // Thin border
        sb.Draw(
            Game1.staminaRect,
            new Rectangle((int)x, (int)y, (int)panelWidth, 1),
            Color.DarkSlateGray * 0.8f);
        sb.Draw(
            Game1.staminaRect,
            new Rectangle((int)x, (int)(y + panelHeight - 1), (int)panelWidth, 1),
            Color.DarkSlateGray * 0.8f);
        sb.Draw(
            Game1.staminaRect,
            new Rectangle((int)x, (int)y, 1, (int)panelHeight),
            Color.DarkSlateGray * 0.8f);
        sb.Draw(
            Game1.staminaRect,
            new Rectangle((int)(x + panelWidth - 1), (int)y, 1, (int)panelHeight),
            Color.DarkSlateGray * 0.8f);
    }

    private static void DrawSeparator(SpriteBatch sb, float x, float y, float width)
    {
        sb.Draw(
            Game1.staminaRect,
            new Rectangle((int)x, (int)y, (int)width, 1),
            Color.Gray * 0.6f);
    }

    private static void DrawText(
        SpriteBatch sb,
        string text,
        float x,
        float y,
        Color color,
        float scale)
    {
        // Shadow
        sb.DrawString(
            Game1.smallFont,
            text,
            new Vector2(x + 1, y + 1),
            Color.Black * 0.5f,
            0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None,
            0f);

        // Main text
        sb.DrawString(
            Game1.smallFont,
            text,
            new Vector2(x, y),
            color,
            0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None,
            0f);
    }

    private static string Capitalize(string s) =>
        char.ToUpperInvariant(s[0]) + s.Substring(1);
}