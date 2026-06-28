using AgeRegression.Systems;
using AgeRegression.Utilities;
using HarmonyLib;
using StardewValley;

namespace AgeRegression.Patches;

/// <summary>
/// Postfix patch on <see cref="Farmer.MaxStamina" /> property getter
/// that applies the regression energy multiplier.
///
/// <para>
/// Stardew Valley 1.6 exposes <c>Farmer.MaxStamina</c> as a property.
/// We patch the getter so any system reading max stamina sees the
/// regression-adjusted value without us needing to modify the
/// underlying field.
/// </para>
///
/// <para>
/// Note: mood does not affect <c>MaxEnergyMultiplier</c> by design —
/// only regression stage modifiers affect the energy cap.
/// </para>
/// </summary>
[HarmonyPatch(typeof(Farmer), nameof(Farmer.MaxStamina),
    MethodType.Getter)]
internal static class FarmerStaminaPatch
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
    private static void Postfix(Farmer __instance, ref int __result)
    {
        if (_regressionSystem is null) return;
        if (!__instance.IsLocalPlayer) return;

        var modifiers = _regressionSystem.GetEffectiveModifiers();
        if (Math.Abs(modifiers.MaxEnergyMultiplier - 1f) < 0.001f)
            return;

        __result = (int)Math.Round(__result * modifiers.MaxEnergyMultiplier);

        // Ensure current stamina never exceeds the new cap
        if (__instance.Stamina > __result)
            __instance.Stamina = __result;
    }
}
