using AgeRegression.Data;
using AgeRegression.State;
using AgeRegression.Utilities;

namespace AgeRegression.Systems;

/// <summary>
/// <see cref="IStatModifierProvider"/> implementation that contributes
/// stat modifiers based on the player's current mood level.
///
/// <para>
/// Mood is derived from comfort score by <see cref="MoodSystem"/>.
/// This provider reads the current mood from
/// <see cref="StateManager"/> and looks up its stat modifiers from
/// <see cref="DataLoader"/>.
/// </para>
///
/// <para>
/// Note: mood does not affect <c>MaxEnergyMultiplier</c> or tool use
/// by design — mood affects speed and focus (XP), not stamina capacity.
/// </para>
/// </summary>
public sealed class MoodStatModifierProvider : IStatModifierProvider
{
    private readonly StateManager _stateManager;
    private readonly DataLoader _dataLoader;
    private readonly LogHelper _log;

    public string ProviderId => "mood";

    public MoodStatModifierProvider(
        StateManager stateManager,
        DataLoader dataLoader,
        LogHelper log)
    {
        _stateManager = stateManager;
        _dataLoader   = dataLoader;
        _log          = log;
    }

    public StatModifierContribution GetContribution()
    {
        var state = _stateManager.GetCurrentState();
        if (state is null)
            return StatModifierContribution.Identity;

        if (string.IsNullOrEmpty(state.Mood.CurrentMoodId))
            return StatModifierContribution.Identity;

        var mood = _dataLoader.GetMoodLevel(state.Mood.CurrentMoodId);
        if (mood is null)
        {
            _log.Trace(
                "MoodStatModifierProvider: mood '{0}' not found in data.",
                state.Mood.CurrentMoodId);
            return StatModifierContribution.Identity;
        }

        return new StatModifierContribution(
            mood.StatModifiers.SpeedMultiplier,
            1f,
            mood.StatModifiers.SkillXpMultiplier,
            true);
    }
}
