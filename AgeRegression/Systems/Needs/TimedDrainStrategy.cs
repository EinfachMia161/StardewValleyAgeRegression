using AgeRegression.State;

namespace AgeRegression.Systems.Needs;

/// <summary>
/// Simple time-based drain strategy. Drains at a constant rate per
/// hour regardless of player state. Used for hunger and thirst.
/// </summary>
public sealed class TimedDrainStrategy : INeedsDrainStrategy
{
    private readonly float _drainPerHour;

    /// <param name="drainPerHour">
    /// Normalized drain per in-game hour (0.0–1.0 scale).
    /// </param>
    public TimedDrainStrategy(float drainPerHour)
    {
        _drainPerHour = drainPerHour;
    }

    /// <inheritdoc />
    public float ComputeDrain(PlayerRegressionState state, int elapsedMinutes)
    {
        if (elapsedMinutes <= 0) return 0f;

        var hours = elapsedMinutes / 60f;
        return _drainPerHour * hours;
    }
}
