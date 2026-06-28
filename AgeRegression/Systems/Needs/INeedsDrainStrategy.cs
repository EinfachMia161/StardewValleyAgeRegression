using AgeRegression.State;

namespace AgeRegression.Systems.Needs;

/// <summary>
/// Strategy interface for computing how much a needs value drains
/// (or recovers) per tick.
///
/// <para>
/// Implementations receive the full player state so they can factor in
/// regression stage, comfort, equipped items, etc. without the
/// <see cref="NeedsValue"/> itself needing to know about those systems.
/// </para>
/// </summary>
public interface INeedsDrainStrategy
{
    /// <summary>
    /// Computes the normalized drain delta for one tick.
    /// Positive values drain the need; negative values recover it.
    /// </summary>
    /// <param name="state">The current player state.</param>
    /// <param name="elapsedMinutes">
    /// In-game minutes elapsed since the last tick.
    /// </param>
    /// <returns>
    /// Normalized drain amount (0.0–1.0 scale).
    /// Return 0 to skip this tick.
    /// </returns>
    float ComputeDrain(PlayerRegressionState state, int elapsedMinutes);
}
