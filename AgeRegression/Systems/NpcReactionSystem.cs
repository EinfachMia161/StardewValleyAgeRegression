using AgeRegression.Config;
using AgeRegression.Dialogue;
using AgeRegression.Events;
using AgeRegression.State;
using AgeRegression.Utilities;
using StardewValley;

namespace AgeRegression.Systems;

/// <summary>
/// Hooks into NPC interaction events and injects regression-aware
/// dialogue when the player talks to an NPC.
///
/// <para>
/// This system delegates all dialogue selection to
/// <see cref="DialogueManager"/> — no dialogue logic lives here.
/// </para>
/// </summary>
public sealed class NpcReactionSystem
{
    private readonly DialogueManager _dialogueManager;
    private readonly StateManager _stateManager;
    private readonly ModConfig _config;
    private readonly ModEventBus _eventBus;
    private readonly LogHelper _log;

    public NpcReactionSystem(
        DialogueManager dialogueManager,
        StateManager stateManager,
        ModConfig config,
        ModEventBus eventBus,
        LogHelper log)
    {
        _dialogueManager = dialogueManager;
        _stateManager    = stateManager;
        _config          = config;
        _eventBus        = eventBus;
        _log             = log;
    }

    public string? TryInjectDialogue(StardewValley.NPC npc)
    {
        if (!_config.Enabled || !_config.Npcs.NpcReactionsEnabled)
            return null;

        if (!GameHelper.IsWorldReady()) return null;

        var state = _stateManager.GetCurrentState();
        if (state is null) return null;

        if (!_stateManager.IsRegressed()) return null;

        return _dialogueManager.TryGetDialogue(npc);
    }
}
