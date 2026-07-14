namespace AgeRegression.Data;

/// <summary>
/// Immutable unlock condition requiring the current season to match the
/// specified value (spring, summer, fall, winter), lower-case.
/// </summary>
public sealed class SeasonCondition : IUnlockCondition
{
    /// <summary>Required season name, lower-case.</summary>
    public string RequiredSeason { get; }

    public SeasonCondition(string requiredSeason)
    {
        RequiredSeason = requiredSeason;
    }
}
