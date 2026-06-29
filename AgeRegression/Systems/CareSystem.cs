using AgeRegression.Data;
using AgeRegression.Events;
using AgeRegression.State;
using AgeRegression.Utilities;

namespace AgeRegression.Systems;

/// <summary>
/// Tracks player care history for roleplay and dialogue context.
/// NOT an achievement or progression system.
///
/// <para>
/// Subscribes to diaper changes to record care actions.
/// Publishes <see cref="CareActionCompletedEventArgs"/> for systems
/// that need to react to care events.
/// </para>
/// </summary>
public sealed class CareSystem
{
    private readonly StateManager _stateManager;
    private readonly ModEventBus _eventBus;
    private readonly LogHelper _log;

    public CareSystem(
        StateManager stateManager,
        ModEventBus eventBus,
        LogHelper log)
    {
        _stateManager = stateManager;
        _eventBus     = eventBus;
        _log          = log;

        eventBus.Subscribe<DiaperStateChangedEventArgs>(OnDiaperStateChanged);
    }

    public void OnDayStarted()
    {
        var state = _stateManager.GetCurrentState();
        if (state is null) return;

        var currentDay = AbsoluteDayHelper.GetCurrentAbsoluteDay();
        if (state.Care.LastCareDayAbsolute != currentDay)
        {
            state.Care.CareActionsToday = 0;
            state.Care.LastCareDayAbsolute = currentDay;

            _log.Debug("CareSystem: care actions reset for new day.");
        }
    }

    public void RecordCareAction(string careActionId, bool recordDiaperChange = false, string? context = null)
    {
        var state = _stateManager.GetCurrentState();
        if (state is null) return;

        var currentDay = AbsoluteDayHelper.GetCurrentAbsoluteDay();
        var location = GameHelper.GetCurrentLocationName();

        state.Care.CareActionsToday++;
        state.Care.LastCareActionId = careActionId;

        if (recordDiaperChange)
        {
            state.Care.LastDiaperChangeAbsoluteDay = currentDay;
        }

        _eventBus.Publish(new CareActionCompletedEventArgs(
            careActionId, state.PlayerId, currentDay, location, context));

        _log.Info(
            "Care action recorded: '{0}' (count today: {1}).",
            careActionId, state.Care.CareActionsToday);
    }

    private void OnDiaperStateChanged(DiaperStateChangedEventArgs e)
    {
        var previousWasWearing = e.PreviousState.IsWearingDiaper;
        var newIsWearing = e.NewState.IsWearingDiaper;

        if (!newIsWearing && previousWasWearing)
        {
            RecordCareAction("diaper_removed");
            return;
        }

        if (newIsWearing && !previousWasWearing)
        {
            var diaperTypeId = e.NewState.EquippedDiaperTypeId ?? "unknown";
            RecordCareAction($"diaper_equipped_{diaperTypeId}", true);
            return;
        }

        if (newIsWearing && previousWasWearing &&
            e.NewState.EquippedDiaperTypeId != e.PreviousState.EquippedDiaperTypeId)
        {
            var diaperTypeId = e.NewState.EquippedDiaperTypeId ?? "unknown";
            RecordCareAction($"diaper_changed_{diaperTypeId}", true);
        }
    }
}