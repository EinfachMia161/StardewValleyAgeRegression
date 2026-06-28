using AgeRegression.Data;
using AgeRegression.Events;
using AgeRegression.State;
using AgeRegression.Utilities;

namespace AgeRegression.Api;

/// <summary>
/// Implementation of <see cref="IAgeRegressionApi" /> exposed to other
/// mods. Bridges the internal event bus to standard .NET events for
/// external consumers.
///
/// <para>
/// Subscribes to the internal event bus in the constructor so the
/// bridge is always active and cannot be forgotten.
/// </para>
/// </summary>
public sealed class AgeRegressionApi : IAgeRegressionApi
{
    private readonly StateManager _stateManager;
    private readonly DataLoader _dataLoader;
    private readonly LogHelper _log;

    public AgeRegressionApi(
        StateManager stateManager,
        DataLoader dataLoader,
        ModEventBus eventBus,
        LogHelper log)
    {
        _stateManager = stateManager;
        _dataLoader   = dataLoader;
        _log          = log;

        // Subscribe immediately — no separate wiring step required
        eventBus.Subscribe<RegressionChangedEventArgs>(
            OnRegressionChanged);
        eventBus.Subscribe<DiaperStateChangedEventArgs>(
            OnDiaperStateChanged);
    }

    // -------------------------------------------------------------------------
    // IAgeRegressionApi
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public string? GetCurrentStageId() =>
        _stateManager.GetCurrentState()?.CurrentStageId;

    /// <inheritdoc />
    public IReadOnlyList<RegressionStageData> GetAllStages() =>
        _dataLoader.Stages;

    /// <inheritdoc />
    public bool IsRegressed() =>
        _stateManager.IsRegressed();

    /// <inheritdoc />
    public bool IsWearingDiaper() =>
        _stateManager.GetCurrentState()?.Diaper.IsWearingDiaper ?? false;

    /// <inheritdoc />
    public string GetDiaperCondition()
    {
        var state = _stateManager.GetCurrentState();
        return state?.Diaper.IsWearingDiaper == true
            ? state.Diaper.ConditionId
            : "none";
    }

    /// <inheritdoc />
    public float GetComfortScore() =>
        _stateManager.GetCurrentState()?.Comfort.CurrentComfort ?? 0f;

    /// <inheritdoc />
    public bool TrySetStage(string stageId)
    {
        _log.Debug("External API: TrySetStage('{0}').", stageId);
        return _stateManager.TrySetStage(stageId);
    }

    // -------------------------------------------------------------------------
    // Public events
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public event EventHandler<RegressionStageChangedApiEventArgs>?
        RegressionStageChanged;

    /// <inheritdoc />
    public event EventHandler<DiaperStateChangedApiEventArgs>?
        DiaperStateChanged;

    // -------------------------------------------------------------------------
    // Internal event bridge
    // -------------------------------------------------------------------------

    private void OnRegressionChanged(RegressionChangedEventArgs e)
    {
        RegressionStageChanged?.Invoke(this,
            new RegressionStageChangedApiEventArgs
            {
                PreviousStageId = e.PreviousStage.Id,
                NewStageId      = e.NewStage.Id
            });
    }

    private void OnDiaperStateChanged(DiaperStateChangedEventArgs e)
    {
        DiaperStateChanged?.Invoke(this,
            new DiaperStateChangedApiEventArgs
            {
                IsWearingDiaper = e.NewState.IsWearingDiaper,
                ConditionId     = e.NewState.ConditionId
            });
    }
}
