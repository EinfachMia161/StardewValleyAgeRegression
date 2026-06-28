using AgeRegression.Config;
using AgeRegression.Data;
using AgeRegression.Events;
using AgeRegression.State;
using AgeRegression.Utilities;
using System.Linq;

namespace AgeRegression.Systems;

/// <summary>
/// Derives the player's mood from their comfort score and fires
/// <see cref="MoodChangedEventArgs"/> when the mood level transitions.
///
/// <para>
/// Mood levels are defined in <c>assets/data/mood-levels.json</c>.
/// Mood stat modifiers stack multiplicatively with regression stage
/// modifiers via <see cref="MoodStatModifierProvider"/> registered
/// with <see cref="RegressionSystem"/>.
/// </para>
/// </summary>
public sealed class MoodSystem
{
    private readonly StateManager _stateManager;
    private readonly DataLoader _dataLoader;
    private readonly ModConfig _config;
    private readonly ModEventBus _eventBus;
    private readonly LogHelper _log;

    public MoodSystem(
        StateManager stateManager,
        DataLoader dataLoader,
        ModConfig config,
        ModEventBus eventBus,
        LogHelper log)
    {
        _stateManager = stateManager;
        _dataLoader   = dataLoader;
        _config       = config;
        _eventBus     = eventBus;
        _log          = log;

        eventBus.Subscribe<ComfortChangedEventArgs>(OnComfortChanged);
    }

    public MoodLevelData? GetCurrentMood()
    {
        var state = _stateManager.GetCurrentState();
        if (state is null) return null;
        return _dataLoader.GetMoodLevel(state.Mood.CurrentMoodId);
    }

    public MoodStatModifiers GetCurrentMoodModifiers()
    {
        return GetCurrentMood()?.StatModifiers ?? new MoodStatModifiers();
    }

    private void OnComfortChanged(ComfortChangedEventArgs e)
    {
        var state = _stateManager.GetCurrentState();
        if (state is null) return;

        var normalized = state.Comfort.GetNormalized(_config.Comfort.MaxComfort);
        var newMood    = FindMoodForComfort(normalized);
        if (newMood is null) return;

        if (newMood.Id == state.Mood.CurrentMoodId) return;

        _log.Debug("Mood changed: '{0}' \u2192 '{1}' (comfort={2:P0}).",
            state.Mood.CurrentMoodId, newMood.Id, normalized);

        _stateManager.UpdateMood(newMood.Id);
    }

    private MoodLevelData? FindMoodForComfort(float normalizedComfort)
    {
        return _dataLoader.MoodLevels
            .Where(m => m.MinComfortNormalized <= normalizedComfort)
            .OrderByDescending(m => m.MinComfortNormalized)
            .FirstOrDefault();
    }
}
