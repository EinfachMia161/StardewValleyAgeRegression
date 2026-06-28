using AgeRegression.Systems;
using AgeRegression.Utilities;
using HarmonyLib;
using StardewValley;

namespace AgeRegression.Patches;

/// <summary>
/// Postfix patch on <see cref="Farmer.getMovementSpeed" /> that applies
/// the regression speed multiplier to the computed movement speed.
///
/// <para>
/// We use a postfix rather than a prefix so we modify the final result
/// after all vanilla speed calculations (buffs, horse, etc.) have run.
/// This means regression scales the final speed rather than a base
/// value, which is the most compatible approach with other mods.
/// </para>
/// </summary>
[HarmonyPatch(typeof(Farmer), nameof(Farmer.getMovementSpeed))]
internal static class FarmerMovementPatch
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

    [HarmonyPostfix]
    private static void Postfix(Farmer __instance, ref float __result)
    {
        if (_regressionSystem is null) return;

        // Only patch the local player
        if (!__instance.IsLocalPlayer) return;

        var modifiers = _regressionSystem.GetEffectiveModifiers();
        if (Math.Abs(modifiers.SpeedMultiplier - 1f) < 0.001f) return;

        __result *= modifiers.SpeedMultiplier;
    }
}
