using AgeRegression.Data;
using AgeRegression.State;

namespace AgeRegression.Systems;

/// <summary>
/// Pure calculation helpers for the diaper system.
/// Extracted into a separate class so they can be unit tested without
/// pulling in SMAPI dependencies.
/// </summary>
public static class DiaperCalculations
{
    /// <summary>
    /// Calculates how much wetness a single accident adds, based on
    /// diaper type.
    ///
    /// <para>
    /// Formula:
    /// <c>referenceIncrease
    ///    × (referenceCapacity / maxCapacity)
    ///    / absorptionRate</c>
    /// </para>
    ///
    /// <para>
    /// Higher capacity diapers fill more slowly per accident.
    /// Higher absorption rates also reduce the wetness increase per
    /// accident (the diaper handles the fluid more efficiently).
    /// </para>
    /// </summary>
    public static float CalculateWetnessIncrease(DiaperTypeData diaperType)
    {
        const float referenceCapacity = 100f;
        const float referenceIncrease = 0.20f;

        var capacityFactor   = referenceCapacity
            / Math.Max(diaperType.MaxCapacity, 1f);
        var absorptionFactor = 1f
            / Math.Max(diaperType.AbsorptionRate, 0.1f);

        return referenceIncrease * capacityFactor * absorptionFactor;
    }

    /// <summary>
    /// Calculates the accident chance for a given player state.
    /// Returns 0 if the player is at the baseline stage.
    /// </summary>
    public static float CalculateAccidentChance(
        DiaperState state,
        string currentStageId,
        string baselineStageId,
        int currentStageOrder,
        int maxStageOrder,
        float baseAccidentChance)
    {
        if (currentStageId == baselineStageId) return 0f;
        if (maxStageOrder <= 0) return 0f;

        var depthFactor    = (float)currentStageOrder / maxStageOrder;
        var fullnessFactor = 1f - (state.WetnessLevel * 0.5f);

        return baseAccidentChance * depthFactor * fullnessFactor;
    }
}
