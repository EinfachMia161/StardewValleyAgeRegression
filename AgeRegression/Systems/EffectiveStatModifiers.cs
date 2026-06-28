namespace AgeRegression.Systems;

/// <summary>
/// The fully composed effective stat modifiers for the current player
/// state. Computed by
/// <see cref="RegressionSystem.GetEffectiveModifiers"/> and consumed
/// by Harmony patches.
/// </summary>
public sealed class EffectiveStatModifiers
{
    /// <summary>
    /// A no-op modifier set representing no active regression effects.
    /// </summary>
    public static readonly EffectiveStatModifiers Identity =
        new(1f, 1f, 1f, true);

    /// <summary>Movement speed multiplier.</summary>
    public float SpeedMultiplier { get; }

    /// <summary>Maximum stamina/energy multiplier.</summary>
    public float MaxEnergyMultiplier { get; }

    /// <summary>Skill XP gain multiplier.</summary>
    public float SkillXpMultiplier { get; }

    /// <summary>
    /// Whether the player can use tools at this modifier set.
    /// </summary>
    public bool CanUseTools { get; }

    public EffectiveStatModifiers(
        float speedMultiplier,
        float maxEnergyMultiplier,
        float skillXpMultiplier,
        bool canUseTools)
    {
        SpeedMultiplier     = speedMultiplier;
        MaxEnergyMultiplier = maxEnergyMultiplier;
        SkillXpMultiplier   = skillXpMultiplier;
        CanUseTools         = canUseTools;
    }
}
