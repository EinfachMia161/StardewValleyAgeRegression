namespace AgeRegression.Systems;

/// <summary>
/// Provides a set of stat multipliers that stack into the player's
/// effective stat modifiers alongside regression stage modifiers.
///
/// <para>
/// Implement this interface to contribute stat modifiers from any
/// source: mood, equipped items, furniture proximity, clothing,
/// temporary buffs, NPC relationship bonuses, etc.
/// </para>
///
/// <para>
/// <see cref="RegressionSystem.GetEffectiveModifiers"/> iterates all
/// registered providers and multiplies their contributions together.
/// Providers are stateless from the caller's perspective — they are
/// queried on demand and must return current values synchronously.
/// </para>
///
/// <para>
/// Contract:
/// <list type="bullet">
///   <item>Return <see cref="StatModifierContribution.Identity"/> when
///   the provider has no active effect.</item>
///   <item>Never return multipliers outside the range [0.01, 10.0] —
///   <see cref="RegressionSystem"/> clamps the final result but extreme
///   values from a single provider indicate a data error.</item>
///   <item>Must not throw. Return identity on any error condition.</item>
/// </list>
/// </para>
/// </summary>
public interface IStatModifierProvider
{
    /// <summary>
    /// A short identifier used for logging and debugging.
    /// Example: <c>"mood"</c>, <c>"equipped_items"</c>,
    /// <c>"furniture_proximity"</c>.
    /// </summary>
    string ProviderId { get; }

    /// <summary>
    /// Returns the current stat modifier contribution from this
    /// provider. Called every time
    /// <see cref="RegressionSystem.GetEffectiveModifiers"/> is invoked
    /// — keep this method fast.
    /// </summary>
    StatModifierContribution GetContribution();
}

/// <summary>
/// A set of multiplicative stat modifiers contributed by a single
/// <see cref="IStatModifierProvider"/>.
///
/// <para>
/// All values are multipliers. 1.0 means no effect.
/// Values are multiplied together across all active providers.
/// </para>
/// </summary>
public readonly struct StatModifierContribution
{
    /// <summary>
    /// A no-op contribution. Use this when a provider has no active
    /// effect.
    /// </summary>
    public static readonly StatModifierContribution Identity =
        new(1f, 1f, 1f, true);

    /// <summary>Movement speed multiplier.</summary>
    public float SpeedMultiplier { get; }

    /// <summary>Maximum stamina/energy multiplier.</summary>
    public float MaxEnergyMultiplier { get; }

    /// <summary>Skill XP gain multiplier.</summary>
    public float SkillXpMultiplier { get; }

    /// <summary>
    /// Whether the player can use tools under this contribution.
    /// If any provider returns <c>false</c>, tools are disabled.
    /// </summary>
    public bool CanUseTools { get; }

    public StatModifierContribution(
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

    /// <summary>
    /// Convenience constructor for providers that only affect speed
    /// and XP. Energy and tool use default to no effect.
    /// </summary>
    public StatModifierContribution(float speedMultiplier, float skillXpMultiplier)
        : this(speedMultiplier, 1f, skillXpMultiplier, true) { }

    /// <inheritdoc />
    public override string ToString() =>
        $"[speed={SpeedMultiplier:F2} energy={MaxEnergyMultiplier:F2} " +
        $"xp={SkillXpMultiplier:F2} tools={CanUseTools}]";
}
