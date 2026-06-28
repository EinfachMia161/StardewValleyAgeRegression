using AgeRegression.Config;
using AgeRegression.Data;
using AgeRegression.State;

namespace AgeRegression.Systems.Needs;

/// <summary>
/// Continence drain strategy. Drain rate is influenced by:
/// <list type="bullet">
///   <item>Regression stage depth (deeper = faster drain)</item>
///   <item>Comfort/stress state (more stress = faster drain)</item>
///   <item>Equipped diaper type
///   (<see cref="DiaperTypeData.ContinenceDrainMultiplier"/>)</item>
///   <item>Whether the player is at baseline (recovery instead of
///   drain)</item>
/// </list>
///
/// <para>
/// All multiplier values come from data files. No values are hardcoded.
/// The pure calculation logic is exposed via
/// <see cref="ContinenceDrainCalculator"/> for unit testing without
/// requiring a live <see cref="DataLoader"/>.
/// </para>
/// </summary>
public sealed class ContinenceDrainStrategy : INeedsDrainStrategy
{
    private readonly ContinenceConfig _config;
    private readonly DataLoader _dataLoader;

    public ContinenceDrainStrategy(
        ContinenceConfig config,
        DataLoader dataLoader)
    {
        _config     = config;
        _dataLoader = dataLoader;
    }

    /// <inheritdoc />
    public float ComputeDrain(PlayerRegressionState state, int elapsedMinutes)
    {
        if (elapsedMinutes <= 0) return 0f;

        var baseline = _dataLoader.GetBaselineStage();
        var current  = _dataLoader.GetStage(state.CurrentStageId);

        if (baseline is null || current is null)
            return _config.BaseDrainPerHour * (elapsedMinutes / 60f);

        var maxOrder = _dataLoader.Stages.Count > 0
            ? _dataLoader.Stages.Max(s => s.Order)
            : 1;

        var diaperMultiplier = state.Diaper.IsWearingDiaper
            ? _dataLoader.GetDiaperType(state.Diaper.EquippedDiaperTypeId!)
                ?.ContinenceDrainMultiplier ?? 1.0f
            : 1.0f;

        return ContinenceDrainCalculator.Compute(
            elapsedMinutes,
            current.Id,
            baseline.Id,
            current.Order,
            maxOrder,
            state.Needs.Continence.StressModifier,
            diaperMultiplier,
            _config.BaseDrainPerHour,
            _config.MaxRegressionDrainMultiplier,
            _config.StressInfluence,
            _config.RecoveryPerHour);
    }
}

/// <summary>
/// Pure static calculation logic for continence drain.
/// Extracted from <see cref="ContinenceDrainStrategy"/> so it can be
/// unit tested without a live <see cref="DataLoader"/> or game context.
/// </summary>
public static class ContinenceDrainCalculator
{
    /// <summary>
    /// Computes the normalized continence drain for one tick.
    /// Positive = drain. Negative = recovery.
    /// </summary>
    /// <param name="elapsedMinutes">
    /// In-game minutes elapsed since the last tick.
    /// </param>
    /// <param name="currentStageId">
    /// The player's current regression stage ID.
    /// </param>
    /// <param name="baselineStageId">
    /// The baseline (no regression) stage ID.
    /// </param>
    /// <param name="currentStageOrder">
    /// The order value of the current stage.
    /// </param>
    /// <param name="maxStageOrder">
    /// The highest order value across all stages.
    /// </param>
    /// <param name="stressModifier">
    /// The current stress modifier from the continence state.
    /// </param>
    /// <param name="diaperContinenceMultiplier">
    /// The continence drain multiplier from the equipped diaper, or
    /// 1.0 if no diaper is equipped.
    /// </param>
    /// <param name="baseDrainPerHour">
    /// Base drain per hour from config.
    /// </param>
    /// <param name="maxRegressionDrainMultiplier">
    /// Maximum drain multiplier at deepest regression stage.
    /// </param>
    /// <param name="stressInfluence">
    /// How much the stress modifier amplifies drain.
    /// </param>
    /// <param name="recoveryPerHour">
    /// Recovery rate per hour at baseline.
    /// </param>
    public static float Compute(
        int elapsedMinutes,
        string currentStageId,
        string baselineStageId,
        int currentStageOrder,
        int maxStageOrder,
        float stressModifier,
        float diaperContinenceMultiplier,
        float baseDrainPerHour,
        float maxRegressionDrainMultiplier,
        float stressInfluence,
        float recoveryPerHour)
    {
        if (elapsedMinutes <= 0) return 0f;

        var hours = elapsedMinutes / 60f;

        if (currentStageId == baselineStageId)
            return -(recoveryPerHour * hours);

        var depthFactor = maxStageOrder > 0
            ? (float)currentStageOrder / maxStageOrder
            : 0f;

        var regressionMultiplier = 1f +
            (depthFactor * (maxRegressionDrainMultiplier - 1f));

        var stressMultiplier = Math.Max(0.1f,
            1f + (stressModifier * stressInfluence));

        var clampedDiaperMultiplier =
            Math.Clamp(diaperContinenceMultiplier, 0.1f, 2.0f);

        return baseDrainPerHour
            * regressionMultiplier
            * stressMultiplier
            * clampedDiaperMultiplier
            * hours;
    }
}
