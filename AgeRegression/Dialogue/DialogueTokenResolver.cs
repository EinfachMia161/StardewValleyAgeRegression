using AgeRegression.Data;

namespace AgeRegression.Dialogue;

/// <summary>
/// Resolves <c>{token}</c> placeholders in dialogue text strings.
/// Tokens are replaced with values derived from the current
/// <see cref="DialogueContext"/>.
///
/// <para>
/// Token format: <c>{TokenName}</c> (case-insensitive).
/// Unknown tokens are left as-is so dialogue authors can see them
/// during development.
/// </para>
/// </summary>
public sealed class DialogueTokenResolver
{
    private readonly DataLoader _dataLoader;

    public DialogueTokenResolver(DataLoader dataLoader)
    {
        _dataLoader = dataLoader;
    }

    /// <summary>
    /// Resolves all tokens in <paramref name="text"/> using the given
    /// context.
    /// </summary>
    public string Resolve(string text, DialogueContext context)
    {
        if (!text.Contains('{')) return text;

        return text
            .Replace("{PlayerName}",
                GetPlayerName(),
                StringComparison.OrdinalIgnoreCase)
            .Replace("{RegressionStage}",
                GetStageName(context),
                StringComparison.OrdinalIgnoreCase)
            .Replace("{DiaperCondition}",
                CapitalizeFirst(context.DiaperConditionId),
                StringComparison.OrdinalIgnoreCase)
            .Replace("{NpcName}",
                context.NpcName,
                StringComparison.OrdinalIgnoreCase)
            .Replace("{Season}",
                CapitalizeFirst(context.Season),
                StringComparison.OrdinalIgnoreCase)
            .Replace("{ContinenceLevel}",
                FormatPercent(context.ContinenceNormalized),
                StringComparison.OrdinalIgnoreCase)
            .Replace("{HungerLevel}",
                FormatPercent(context.HungerNormalized),
                StringComparison.OrdinalIgnoreCase)
            .Replace("{ThirstLevel}",
                FormatPercent(context.ThirstNormalized),
                StringComparison.OrdinalIgnoreCase)
            .Replace("{ComfortLevel}",
                FormatPercent(context.ComfortNormalized),
                StringComparison.OrdinalIgnoreCase);
    }

    private static string GetPlayerName() =>
        StardewValley.Game1.player?.Name ?? "Farmer";

    private string GetStageName(DialogueContext context)
    {
        var stage = _dataLoader.GetStage(context.RegressionStageId);
        return stage?.DisplayName ?? context.RegressionStageId;
    }

    private static string CapitalizeFirst(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpperInvariant(s[0]) + s[1..];
    }

    private static string FormatPercent(float normalized) =>
        $"{(int)(normalized * 100)}%";
}
