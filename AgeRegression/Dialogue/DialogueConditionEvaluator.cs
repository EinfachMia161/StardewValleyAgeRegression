using AgeRegression.Data;

namespace AgeRegression.Dialogue;

/// <summary>
/// Evaluates whether a <see cref="DialogueConditions"/> object is
/// satisfied by a given <see cref="DialogueContext"/>.
///
/// <para>
/// This class is pure — it has no dependencies on game state or SMAPI.
/// All inputs come through the context parameter, making it fully
/// testable.
/// </para>
///
/// <para>
/// All non-null condition fields are evaluated as AND conditions.
/// A null field means "no restriction on this dimension."
/// </para>
/// </summary>
public sealed class DialogueConditionEvaluator
{
    /// <summary>
    /// Returns <c>true</c> if all conditions in
    /// <paramref name="conditions"/> are satisfied by
    /// <paramref name="context"/>.
    /// A null <paramref name="conditions"/> object always returns
    /// <c>true</c>.
    /// </summary>
    public bool Evaluate(
        DialogueConditions? conditions,
        DialogueContext context)
    {
        if (conditions is null) return true;

        return CheckRegressionStages(conditions, context)
            && CheckFriendship(conditions, context)
            && CheckMarriage(conditions, context)
            && CheckSeasons(conditions, context)
            && CheckTime(conditions, context)
            && CheckWeather(conditions, context)
            && CheckLocations(conditions, context)
            && CheckDiaperConditions(conditions, context)
            && CheckIsWearingDiaper(conditions, context)
            && CheckAccessories(conditions, context)
            && CheckNpcPersonality(conditions, context)
            && CheckGameFlags(conditions, context)
            && CheckContinenceThreshold(conditions, context)
            && CheckHungerRange(conditions, context)
            && CheckThirstRange(conditions, context)
            && CheckComfortRange(conditions, context);
    }

    private static bool CheckRegressionStages(
        DialogueConditions c, DialogueContext ctx)
    {
        if (c.RegressionStages is null || c.RegressionStages.Count == 0)
            return true;
        return c.RegressionStages.Contains(
            ctx.RegressionStageId, StringComparer.OrdinalIgnoreCase);
    }

    private static bool CheckFriendship(
        DialogueConditions c, DialogueContext ctx)
    {
        if (c.MinFriendshipHearts.HasValue &&
            ctx.FriendshipHearts < c.MinFriendshipHearts.Value)
            return false;
        if (c.MaxFriendshipHearts.HasValue &&
            ctx.FriendshipHearts > c.MaxFriendshipHearts.Value)
            return false;
        return true;
    }

    private static bool CheckMarriage(
        DialogueConditions c, DialogueContext ctx)
    {
        if (c.IsMarried.HasValue && c.IsMarried.Value != ctx.IsMarried)
            return false;
        return true;
    }

    private static bool CheckSeasons(
        DialogueConditions c, DialogueContext ctx)
    {
        if (c.Seasons is null || c.Seasons.Count == 0) return true;
        return c.Seasons.Contains(ctx.Season, StringComparer.OrdinalIgnoreCase);
    }

    private static bool CheckTime(
        DialogueConditions c, DialogueContext ctx)
    {
        if (c.TimeFrom.HasValue && ctx.TimeOfDay < c.TimeFrom.Value)
            return false;
        if (c.TimeTo.HasValue && ctx.TimeOfDay > c.TimeTo.Value)
            return false;
        return true;
    }

    private static bool CheckWeather(
        DialogueConditions c, DialogueContext ctx)
    {
        if (c.Weather is null || c.Weather.Count == 0) return true;
        return c.Weather.Contains(ctx.Weather, StringComparer.OrdinalIgnoreCase);
    }

    private static bool CheckLocations(
        DialogueConditions c, DialogueContext ctx)
    {
        if (c.Locations is null || c.Locations.Count == 0) return true;
        return c.Locations.Contains(
            ctx.LocationName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool CheckDiaperConditions(
        DialogueConditions c, DialogueContext ctx)
    {
        if (c.DiaperConditions is null || c.DiaperConditions.Count == 0)
            return true;
        return c.DiaperConditions.Contains(
            ctx.DiaperConditionId, StringComparer.OrdinalIgnoreCase);
    }

    private static bool CheckIsWearingDiaper(
        DialogueConditions c, DialogueContext ctx)
    {
        if (c.IsWearingDiaper.HasValue &&
            c.IsWearingDiaper.Value != ctx.IsWearingDiaper)
            return false;
        return true;
    }

    private static bool CheckAccessories(
        DialogueConditions c, DialogueContext ctx)
    {
        if (c.RequiredAccessories is null ||
            c.RequiredAccessories.Count == 0)
            return true;
        return c.RequiredAccessories.All(a =>
            ctx.EquippedAccessories.Contains(a));
    }

    private static bool CheckNpcPersonality(
        DialogueConditions c, DialogueContext ctx)
    {
        if (c.NpcPersonalityTags is null ||
            c.NpcPersonalityTags.Count == 0)
            return true;
        return c.NpcPersonalityTags.Any(tag =>
            ctx.NpcPersonalityTags.Contains(
                tag, StringComparer.OrdinalIgnoreCase));
    }

    private static bool CheckGameFlags(
        DialogueConditions c, DialogueContext ctx)
    {
        if (c.RequiredGameFlags is null ||
            c.RequiredGameFlags.Count == 0)
            return true;
        return c.RequiredGameFlags.All(flag =>
            ctx.GameFlags.Contains(flag));
    }

    private static bool CheckContinenceThreshold(
        DialogueConditions c, DialogueContext ctx)
    {
        if (c.ContinenceThresholds is null ||
            c.ContinenceThresholds.Count == 0)
            return true;
        return c.ContinenceThresholds.Contains(
            ctx.ContinenceThresholdId, StringComparer.OrdinalIgnoreCase);
    }

    private static bool CheckHungerRange(
        DialogueConditions c, DialogueContext ctx)
    {
        if (c.MaxHungerNormalized.HasValue &&
            ctx.HungerNormalized > c.MaxHungerNormalized.Value)
            return false;
        if (c.MinHungerNormalized.HasValue &&
            ctx.HungerNormalized < c.MinHungerNormalized.Value)
            return false;
        return true;
    }

    private static bool CheckThirstRange(
        DialogueConditions c, DialogueContext ctx)
    {
        if (c.MaxThirstNormalized.HasValue &&
            ctx.ThirstNormalized > c.MaxThirstNormalized.Value)
            return false;
        if (c.MinThirstNormalized.HasValue &&
            ctx.ThirstNormalized < c.MinThirstNormalized.Value)
            return false;
        return true;
    }

    private static bool CheckComfortRange(
        DialogueConditions c, DialogueContext ctx)
    {
        if (c.MaxComfortNormalized.HasValue &&
            ctx.ComfortNormalized > c.MaxComfortNormalized.Value)
            return false;
        if (c.MinComfortNormalized.HasValue &&
            ctx.ComfortNormalized < c.MinComfortNormalized.Value)
            return false;
        return true;
    }
}
