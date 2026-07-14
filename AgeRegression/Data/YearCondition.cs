namespace AgeRegression.Data;

/// <summary>
/// Immutable unlock condition requiring the player to have reached at least
/// the specified in-game year (1-based).
/// </summary>
public sealed class YearCondition : IUnlockCondition
{
    /// <summary>Minimum required year (1-based).</summary>
    public int RequiredYear { get; }

    public YearCondition(int requiredYear)
    {
        RequiredYear = requiredYear;
    }
}
