namespace AgeRegression.State;

/// <summary>
/// Holds comfort state and the currently active comfort modifiers.
/// </summary>
public sealed class ComfortState
{
    public float CurrentComfort { get; set; }

    public Dictionary<string, float> ActiveModifiers { get; set; } = new();

    public float GetNormalized(float maxComfort)
    {
        if (maxComfort <= 0f) return 0f;
        return Math.Clamp(CurrentComfort / maxComfort, 0f, 1f);
    }
}
