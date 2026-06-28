namespace AgeRegression.State;

/// <summary>
/// Tracks the current mood profile.
/// </summary>
public sealed class MoodState
{
    public string CurrentMoodId { get; set; } = string.Empty;
}
