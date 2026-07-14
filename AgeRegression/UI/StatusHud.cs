using AgeRegression.Config;
using AgeRegression.Data;
using AgeRegression.Events;
using AgeRegression.State;
using AgeRegression.Utilities;
using StardewValley;
using DataLoader = AgeRegression.Data.DataLoader;

namespace AgeRegression.UI;

/// <summary>
/// Manages the in-game status HUD overlay. Subscribes to state change events,
/// caches display data, and provides it to the renderer.
/// </summary>
public sealed class StatusHud
{
    private readonly StateManager _stateManager;
    private readonly DataLoader _dataLoader;
    private readonly ModConfig _config;
    private readonly HudConfig _hudConfig;
    private readonly ModEventBus _eventBus;
    private readonly LogHelper _log;

    private bool _isInitialized;

    // Cached display data — updated only on relevant events
    private string _cachedStageName = "None";
    private float? _cachedProgressPercent;
    private string _cachedDiaperCondition = "None";
    private float _cachedWetnessPercent;
    private float _cachedMessingPercent;
    private float _cachedComfort;
    private string _cachedMood = "Neutral";
    private float _cachedContinence;
    private string? _cachedEquippedDiaperTypeId;
    private SpriteReference? _cachedEquippedDiaperSprite;

    public StatusHud(
        StateManager stateManager,
        DataLoader dataLoader,
        ModConfig config,
        ModEventBus eventBus,
        LogHelper log)
    {
        _stateManager = stateManager;
        _dataLoader   = dataLoader;
        _config       = config;
        _hudConfig    = config.Hud;
        _eventBus     = eventBus;
        _log          = log;

        eventBus.Subscribe<RegressionChangedEventArgs>(OnRegressionChanged);
        eventBus.Subscribe<DiaperStateChangedEventArgs>(OnDiaperStateChanged);
        eventBus.Subscribe<ComfortChangedEventArgs>(OnComfortChanged);
        eventBus.Subscribe<MoodChangedEventArgs>(OnMoodChanged);
        eventBus.Subscribe<NeedsValueChangedEventArgs>(OnNeedsValueChanged);
    }

    /// <summary>
    /// Initializes cached state from the current save. Must be called
    /// after <see cref="StateManager.LoadForCurrentPlayer"/>.
    /// </summary>
    public void OnSaveLoaded()
    {
        var state = _stateManager.GetCurrentState();
        if (state is null)
        {
            _log.Warn("StatusHud.OnSaveLoaded called but no state loaded.");
            _isInitialized = false;
            return;
        }

        RefreshAllCachedValues(state);
        _isInitialized = true;
        _log.Debug("StatusHud initialized.");
    }

    /// <summary>
    /// Returns <c>true</c> if the HUD should be rendered this frame.
    /// </summary>
    public bool ShouldRender()
    {
        if (!_config.Enabled)
            return false;

        if (!_hudConfig.Enabled)
            return false;

        if (!_isInitialized)
            return false;

        if (!GameHelper.IsWorldReady())
            return false;

        return true;
    }

    /// <summary>
    /// Gets the cached display data for rendering.
    /// </summary>
    public HudDisplayData GetDisplayData()
    {
        return new HudDisplayData
        {
            StageName           = _cachedStageName,
            ProgressPercent     = _cachedProgressPercent,
            DiaperCondition     = _cachedDiaperCondition,
            WetnessPercent      = _cachedWetnessPercent,
            MessingPercent      = _cachedMessingPercent,
            Comfort             = _cachedComfort,
            Mood                = _cachedMood,
            Continence          = _cachedContinence,
            EquippedDiaperTypeId = _cachedEquippedDiaperTypeId,
            EquippedDiaperSprite = _cachedEquippedDiaperSprite
        };
    }

    /// <summary>
    /// Gets the current HUD configuration for rendering.
    /// </summary>
    public HudConfig GetConfig() => _hudConfig;

    // -------------------------------------------------------------------------
    // Event handlers — update only the relevant cached values
    // -------------------------------------------------------------------------

    private void OnRegressionChanged(RegressionChangedEventArgs e)
    {
        if (!_isInitialized) return;

        var stage = _dataLoader.GetStage(e.NewStage.Id);
        _cachedStageName = stage?.DisplayName ?? e.NewStage.Id;

        // Calculate progress if we have order information
        var baseline = _dataLoader.GetBaselineStage();
        var maxStage = _dataLoader.Stages
            .OrderByDescending(s => s.Order)
            .FirstOrDefault();

        if (baseline is not null && maxStage is not null)
        {
            var totalRange = maxStage.Order - baseline.Order;
            if (totalRange > 0)
            {
                var currentProgress = stage!.Order - baseline.Order;
                _cachedProgressPercent = (currentProgress / (float)totalRange) * 100f;
            }
            else
            {
                _cachedProgressPercent = null;
            }
        }
        else
        {
            _cachedProgressPercent = null;
        }
    }

    private void OnDiaperStateChanged(DiaperStateChangedEventArgs e)
    {
        if (!_isInitialized) return;

        _cachedDiaperCondition = e.NewState.ConditionId;
        _cachedWetnessPercent  = e.NewState.WetnessLevel * 100f;
        _cachedMessingPercent  = e.NewState.MessingLevel * 100f;

        // Cache diaper type (including its sprite sheet) for the HUD icon
        if (e.NewState.EquippedDiaperTypeId is not null)
        {
            var diaperType = _dataLoader.GetDiaperType(e.NewState.EquippedDiaperTypeId);
            _cachedEquippedDiaperTypeId = e.NewState.EquippedDiaperTypeId;
            _cachedEquippedDiaperSprite = diaperType is null
                ? null
                : new SpriteReference(diaperType.SpriteSheet, diaperType.SpriteIndex);
        }
        else
        {
            _cachedEquippedDiaperTypeId = null;
            _cachedEquippedDiaperSprite = null;
        }
    }

    private void OnComfortChanged(ComfortChangedEventArgs e)
    {
        if (!_isInitialized) return;
        _cachedComfort = e.NewComfort;
    }

    private void OnMoodChanged(MoodChangedEventArgs e)
    {
        if (!_isInitialized) return;

        var mood = _dataLoader.GetMoodLevel(e.NewMoodId);
        _cachedMood = mood?.DisplayName ?? e.NewMoodId;
    }

    private void OnNeedsValueChanged(NeedsValueChangedEventArgs e)
    {
        if (!_isInitialized) return;

        // Continence is tracked in the needs system
        if (e.NeedId == "continence")
        {
            _cachedContinence = e.NewNormalized * 100f;
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private void RefreshAllCachedValues(PlayerRegressionState state)
    {
        // Stage info
        var stage = _dataLoader.GetStage(state.CurrentStageId);
        _cachedStageName = stage?.DisplayName ?? state.CurrentStageId;

        var baseline = _dataLoader.GetBaselineStage();
        var maxStage = _dataLoader.Stages
            .OrderByDescending(s => s.Order)
            .FirstOrDefault();

        if (baseline is not null && maxStage is not null)
        {
            var totalRange = maxStage.Order - baseline.Order;
            if (totalRange > 0 && stage is not null)
            {
                var currentProgress = stage.Order - baseline.Order;
                _cachedProgressPercent = (currentProgress / (float)totalRange) * 100f;
            }
            else
            {
                _cachedProgressPercent = null;
            }
        }
        else
        {
            _cachedProgressPercent = null;
        }

        // Diaper info
        _cachedDiaperCondition = state.Diaper.ConditionId;
        _cachedWetnessPercent  = state.Diaper.WetnessLevel * 100f;
        _cachedMessingPercent  = state.Diaper.MessingLevel * 100f;

        // Comfort
        _cachedComfort = state.Comfort.CurrentComfort;

        // Mood
        var mood = _dataLoader.GetMoodLevel(state.Mood.CurrentMoodId);
        _cachedMood = mood?.DisplayName ?? state.Mood.CurrentMoodId;

        // Continence from needs
        _cachedContinence = state.Needs.Continence.Value.Normalized * 100f;

        // Cache diaper type (including its sprite sheet) for the HUD icon
        if (state.Diaper.EquippedDiaperTypeId is not null)
        {
            var diaperType = _dataLoader.GetDiaperType(state.Diaper.EquippedDiaperTypeId);
            _cachedEquippedDiaperTypeId = state.Diaper.EquippedDiaperTypeId;
            _cachedEquippedDiaperSprite = diaperType is null
                ? null
                : new SpriteReference(diaperType.SpriteSheet, diaperType.SpriteIndex);
        }
        else
        {
            _cachedEquippedDiaperTypeId = null;
            _cachedEquippedDiaperSprite = null;
        }
    }
}

/// <summary>
/// Immutable snapshot of data to render in the HUD.
/// </summary>
public sealed class HudDisplayData
{
    public string StageName { get; init; } = "None";
    public float? ProgressPercent { get; init; }
    public string DiaperCondition { get; init; } = "None";
    public float WetnessPercent { get; init; }
    public float MessingPercent { get; init; }
    public float Comfort { get; init; }
    public string Mood { get; init; } = "Neutral";
    public float Continence { get; init; }
    public string? EquippedDiaperTypeId { get; init; }

    /// <summary>
    /// Sprite reference for the equipped diaper's HUD icon. Derived from the
    /// diaper type's own <c>SpriteSheet</c> and <c>SpriteIndex</c> so the HUD
    /// renders through the same pipeline as world/inventory/held items.
    /// <c>null</c> when no diaper is equipped.
    /// </summary>
    public SpriteReference? EquippedDiaperSprite { get; init; }
}