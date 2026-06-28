using AgeRegression.Config;
using AgeRegression.Data;
using AgeRegression.State;
using AgeRegression.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgeRegression.Dialogue;

/// <summary>
/// Orchestrates dialogue selection for NPC interactions.
///
/// <para>
/// Pipeline:
/// <list type="number">
///   <item>Build <see cref="DialogueContext"/> from live game
///   state.</item>
///   <item>Get the NPC's dialogue pack via
///   <see cref="NpcReactionRouter"/>.</item>
///   <item>Filter entries by
///   <see cref="DialogueConditionEvaluator"/>.</item>
///   <item>Filter by cooldown via
///   <see cref="StateManager"/>.</item>
///   <item>Weighted random selection.</item>
///   <item>Token resolution via
///   <see cref="DialogueTokenResolver"/>.</item>
/// </list>
/// </para>
/// </summary>
public sealed class DialogueManager
{
    private readonly NpcReactionRouter _router;
    private readonly DialogueConditionEvaluator _evaluator;
    private readonly DialogueTokenResolver _tokenResolver;
    private readonly StateManager _stateManager;
    private readonly DataLoader _dataLoader;
    private readonly ModConfig _config;
    private readonly LogHelper _log;

    public DialogueManager(
        NpcReactionRouter router,
        DialogueConditionEvaluator evaluator,
        DialogueTokenResolver tokenResolver,
        StateManager stateManager,
        DataLoader dataLoader,
        ModConfig config,
        LogHelper log)
    {
        _router        = router;
        _evaluator     = evaluator;
        _tokenResolver = tokenResolver;
        _stateManager  = stateManager;
        _dataLoader    = dataLoader;
        _config        = config;
        _log           = log;
    }

    public string? TryGetDialogue(StardewValley.NPC npc)
    {
        if (!_config.Enabled || !_config.Npcs.NpcReactionsEnabled)
            return null;

        var playerState = _stateManager.GetCurrentState();
        if (playerState is null) return null;

        var currentStage = _dataLoader.GetStage(playerState.CurrentStageId);
        if (currentStage is null) return null;

        var npcProfile = _router.GetProfile(npc.Name);
        var pack       = _router.GetDialoguePack(npc.Name);
        if (pack is null || pack.Entries.Count == 0) return null;

        var context = DialogueContext.FromGameState(
            playerState,
            currentStage,
            npcProfile,
            npc,
            _config.Comfort.MaxComfort);

        var entry = SelectEntry(pack.Entries, context, npc.Name);
        if (entry is null) return null;

        _stateManager.RecordDialogueShown(npc.Name, entry.Key);

        var resolved = _tokenResolver.Resolve(entry.Text, context);
        _log.Debug("Dialogue selected for '{0}': key='{1}'.",
            npc.Name, entry.Key);
        return resolved;
    }

    private DialogueEntryData? SelectEntry(
        IReadOnlyList<DialogueEntryData> entries,
        DialogueContext context,
        string npcName)
    {
        var eligible = new List<(DialogueEntryData Entry, int Weight)>();

        foreach (var entry in entries)
        {
            if (!_evaluator.Evaluate(entry.Conditions, context))
                continue;

            if (_stateManager.IsDialogueOnCooldown(
                    npcName, entry.Key, entry.CooldownDays))
                continue;

            var weight = Math.Max(1, entry.Weight);
            eligible.Add((entry, weight));
        }

        if (eligible.Count == 0)
        {
            _log.Trace("No eligible dialogue entries for '{0}'.", npcName);
            return null;
        }

        return WeightedRandom(eligible);
    }

    private static DialogueEntryData WeightedRandom(
        List<(DialogueEntryData Entry, int Weight)> candidates)
    {
        var totalWeight = candidates.Sum(c => c.Weight);
        var roll        = Random.Shared.Next(totalWeight);
        var cumulative  = 0;

        foreach (var (entry, weight) in candidates)
        {
            cumulative += weight;
            if (roll < cumulative)
                return entry;
        }

        return candidates[^1].Entry;
    }
}
