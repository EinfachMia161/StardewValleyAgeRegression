using AgeRegression.Systems;
using AgeRegression.Utilities;
using HarmonyLib;
using StardewValley;

namespace AgeRegression.Patches;

/// <summary>
/// Postfix patch on <see cref="NPC.checkForNewCurrentDialogue" /> that injects
/// regression-aware dialogue when the player interacts with an NPC.
///
/// <para>
/// <b>SV 1.6 API verification:</b>
/// The <c>Dialogue</c> class in SV 1.6 has the constructor:
/// <c>Dialogue(NPC speaker, string dialogueKey, string dialogueText)</c>.
/// <c>NPC.CurrentDialogue</c> is <c>Stack<Dialogue></c> and
/// accepts <c>Push</c> directly. Both are confirmed present in
/// SV 1.6 / SMAPI 4.x.
/// </para>
///
/// <para>
/// We use <c>Push</c> rather than <c>NPC.setNewDialogue</c> because
/// <c>setNewDialogue</c> replaces the entire dialogue stack, discarding
/// any vanilla dialogue that was loaded. <c>Push</c> prepends our line
/// so it appears first while preserving the vanilla follow-up.
/// </para>
///
/// <para>
/// The <c>dialogueKey</c> parameter is an arbitrary string used only
/// for internal SV tracking. We use a mod-namespaced key to avoid
/// collisions.
/// </para>
/// </summary>
[HarmonyPatch(typeof(NPC), nameof(NPC.checkForNewCurrentDialogue))]
internal static class NpcDialoguePatch
{
    private static NpcReactionSystem? _npcReactionSystem;
    private static LogHelper? _log;

    internal static void Initialize(
        NpcReactionSystem system,
        LogHelper log)
    {
        _npcReactionSystem = system;
        _log               = log;
    }

    [HarmonyPostfix]
    private static void Postfix(NPC __instance)
    {
        if (_npcReactionSystem is null) return;

        // Guard: only run when a save is loaded and the world is ready
        if (!StardewModdingAPI.Context.IsWorldReady) return;

        var dialogue = _npcReactionSystem.TryInjectDialogue(__instance);
        if (dialogue is null) return;

        try
        {
            // SV 1.6 confirmed constructor:
            // Dialogue(NPC speaker, string key, string text)
            // The key is namespaced to avoid collisions with vanilla
            // dialogue tracking.
            var dialogueObj = new StardewValley.Dialogue(
                __instance,
                "mia.AgeRegression_Injected",
                dialogue);

            __instance.CurrentDialogue.Push(dialogueObj);

            _log?.Debug("Injected regression dialogue for '{0}'.",
                __instance.Name);
        }
        catch (Exception ex)
        {
            // If the Dialogue constructor signature changes in a future
            // SV update, this catch prevents a hard crash.
            _log?.Exception(
                $"Failed to inject dialogue for '{__instance.Name}'. " +
                "This may indicate a Stardew Valley API change.",
                ex);
        }
    }
}
