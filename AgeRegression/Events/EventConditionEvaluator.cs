using AgeRegression.Config;
using AgeRegression.Data;
using AgeRegression.Dialogue;
using AgeRegression.State;
using AgeRegression.Utilities;

namespace AgeRegression.Events;

/// <summary>
/// Evaluates whether a mod-defined event's conditions are currently met.
///
/// <para>
/// Combines mod-specific <see cref="DialogueConditions"/> evaluation
/// (via <see cref="DialogueConditionEvaluator"/>) with SV-native
/// precondition checking via <c>GameLocation.checkEventPrecondition</c>.
/// </para>
/// </summary>
public sealed class EventConditionEvaluator
{
    private readonly DialogueConditionEvaluator _conditionEvaluator;
    private readonly DataLoader _dataLoader;
    private readonly ModConfig _config;

    public EventConditionEvaluator(
        DialogueConditionEvaluator conditionEvaluator,
        DataLoader dataLoader,
        ModConfig config)
    {
        _conditionEvaluator = conditionEvaluator;
        _dataLoader = dataLoader;
        _config = config;
    }

    /// <summary>
    /// Returns <c>true</c> if all conditions for the given event are met.
    /// </summary>
    public bool AreConditionsMet(EventDefinitionData eventDef, PlayerRegressionState state)
    {
        if (!_config.Enabled) return false;

        // Check mod conditions using the dialogue condition evaluator
        if (eventDef.ModConditions is not null)
        {
            var stage = _dataLoader.GetStage(state.CurrentStageId);
            if (stage is null) return false;

            var context = BuildContext(state, stage);
            if (!_conditionEvaluator.Evaluate(eventDef.ModConditions, context))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Returns <c>true</c> if the event has already been seen
    /// (for one-time events).
    /// </summary>
    public static bool HasBeenSeen(string eventId)
    {
        if (!GameHelper.IsWorldReady()) return false;
        return StardewValley.Game1.player.eventsSeen.Contains(eventId);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private DialogueContext BuildContext(
        PlayerRegressionState state,
        RegressionStageData stage)
    {
        var currentDay = AbsoluteDayHelper.GetCurrentAbsoluteDay();
        var daysSinceLastDiaperChange = AbsoluteDayHelper.DaysBetween(
            state.Care.LastDiaperChangeAbsoluteDay, currentDay);

        return new DialogueContext
        {
            RegressionStageId         = stage.Id,
            RegressionStageOrder      = stage.Order,
            DiaperConditionId         = state.Diaper.IsWearingDiaper
                ? state.Diaper.ConditionId : "none",
            IsWearingDiaper           = state.Diaper.IsWearingDiaper,
            EquippedDiaperTypeId      = state.Diaper.EquippedDiaperTypeId,
            ContinenceNormalized      = state.Needs.Continence.Value.Normalized,
            ContinenceThresholdId     = state.Needs.Continence.Value.LastKnownThresholdId,
            HungerNormalized          = state.Needs.Hunger.Normalized,
            ThirstNormalized          = state.Needs.Thirst.Normalized,
            ComfortNormalized         = state.Comfort.GetNormalized(_config.Comfort.MaxComfort),
            EquippedAccessories       = state.EquippedAccessories,
            DaysSinceLastDiaperChange = daysSinceLastDiaperChange,
            CareActionsToday          = state.Care.CareActionsToday,
            LastCareActionId          = state.Care.LastCareActionId,
            NpcName                   = string.Empty,
            FriendshipHearts          = 0,
            IsMarried                 = !string.IsNullOrEmpty(StardewValley.Game1.player.spouse),
            NpcPersonalityTags        = Array.Empty<string>(),
            Season                    = GameHelper.GetCurrentSeason(),
            TimeOfDay                 = GameHelper.GetCurrentTime(),
            LocationName              = GameHelper.GetCurrentLocationName(),
            Weather                   = "sunny",
            IsFestivalDay             = false,
            GameFlags                 = new HashSet<string>()
        };
    }
}