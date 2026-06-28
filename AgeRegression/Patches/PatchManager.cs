using AgeRegression.Systems;
using AgeRegression.Utilities;
using HarmonyLib;
using StardewModdingAPI;

namespace AgeRegression.Patches;

/// <summary>
/// Applies all Harmony patches for the mod.
/// Each patch class is self-contained. This class only orchestrates
/// registration and logs any conflicts.
/// </summary>
public sealed class PatchManager
{
    private readonly IModHelper _helper;
    private readonly LogHelper _log;
    private Harmony? _harmony;

    public PatchManager(IModHelper helper, LogHelper log)
    {
        _helper = helper;
        _log    = log;
    }

    /// <summary>
    /// Applies all patches. Must be called after all systems are
    /// initialized so patch static references can be set.
    /// </summary>
    public void ApplyAll(
        RegressionSystem regressionSystem,
        NpcReactionSystem npcReactionSystem)
    {
        _harmony = new Harmony("mia.AgeRegression");

        FarmerMovementPatch.Initialize(regressionSystem, _log);
        FarmerStaminaPatch.Initialize(regressionSystem, _log);
        FarmerExperiencePatch.Initialize(regressionSystem, _log);
        NpcDialoguePatch.Initialize(npcReactionSystem, _log);

        try
        {
            _harmony.PatchAll(typeof(PatchManager).Assembly);
            _log.Info("Harmony patches applied successfully.");
        }
        catch (Exception ex)
        {
            _log.Exception("Failed to apply Harmony patches", ex);
        }

        LogPatchedMethods();
    }

    /// <summary>
    /// Removes all patches applied by this mod. Used for clean unload.
    /// </summary>
    public void UnpatchAll()
    {
        _harmony?.UnpatchAll("mia.AgeRegression");
        _log.Debug("All Harmony patches removed.");
    }

    private void LogPatchedMethods()
    {
        if (_harmony is null) return;

        var patched = _harmony.GetPatchedMethods().ToList();
        _log.Debug("Patched {0} method(s):", patched.Count);
        foreach (var method in patched)
            _log.Trace("  → {0}.{1}",
                method.DeclaringType?.Name ?? "?", method.Name);
    }
}
