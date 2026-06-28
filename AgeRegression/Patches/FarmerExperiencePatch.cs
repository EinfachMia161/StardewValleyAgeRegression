using AgeRegression.Systems;
using AgeRegression.Utilities;
using HarmonyLib;
using StardewValley;

namespace AgeRegression.Patches;

/// <summary>
/// Prefix patch on <see cref="Farmer.gainExperience" /> that scales the
/// XP amount by the regression skill XP multiplier before it is
/// applied.
///
/// <para>
/// We use a prefix here so we modify the <c>howMuch</c> parameter
/// before the vanilla method processes it. This is cleaner than a
/// postfix because <c>gainExperience</c> has side effects (level-up
/// checks) that we do not want to interfere with.
/// </para>
/// </summary>
[HarmonyPatch(typeof(Farmer), nameof(Farmer.gainExperience))]
internal static class FarmerExperiencePatch
{
    private static RegressionSystem? _regressionSystem;
    private static LogHelper? _log;

    internal static void Initialize(
        RegressionSystem system,
        LogHelper log)
    {
        _regressionSystem = system;
        _log              = log;
    }

    [HarmonyPrefix]
    private static void Prefix(Farmer __instance, ref int howMuch)
    {
        if (_regressionSystem is null) return;
        if (!__instance.IsLocalPlayer) return;
        if (howMuch <= 0) return;

        var modifiers = _regressionSystem.GetEffectiveModifiers();
        if (Math.Abs(modifiers.SkillXpMultiplier - 1f) < 0.001f) return;

        howMuch = Math.Max(1,
            (int)Math.Round(howMuch * modifiers.SkillXpMultiplier));
    }
}
