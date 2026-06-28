namespace AgeRegression.Events;

/// <summary>
/// Published by <see cref="Systems.DiaperSystem"/> when a diaper
/// interaction (equip, change, or remove) should produce a comfort
/// effect.
///
/// <para>
/// <see cref="Systems.ComfortSystem"/> subscribes to this event and
/// calls <see cref="Systems.ComfortSystem.ApplyDirectAdjustment"/> in
/// response. This decouples <see cref="Systems.DiaperSystem"/> from
/// <see cref="Systems.ComfortSystem"/> entirely — neither class holds
/// a reference to the other.
/// </para>
///
/// <para>
/// Event flow:
/// <code>
/// DiaperSystem
///   ? publishes DiaperComfortEffectEventArgs(delta, reason)
///     ? ComfortSystem.OnDiaperComfortEffect
///       ? ComfortSystem.ApplyDirectAdjustment(delta, reason)
/// </code>
/// </para>
/// </summary>
public sealed class DiaperComfortEffectEventArgs
{
    /// <summary>
    /// The comfort delta to apply.
    /// Positive values increase comfort; negative values decrease it.
    /// </summary>
    public float Delta { get; }

    /// <summary>
    /// A short description of what caused this comfort effect.
    /// Used for logging and debugging.
    /// Example: <c>"diaper_equipped_basic"</c>,
    /// <c>"diaper_removed_premium"</c>.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// The unique ID of the player whose comfort should change.
    /// </summary>
    public long PlayerId { get; }

    public DiaperComfortEffectEventArgs(float delta, string reason, long playerId)
    {
        Delta    = delta;
        Reason   = reason;
        PlayerId = playerId;
    }
}
