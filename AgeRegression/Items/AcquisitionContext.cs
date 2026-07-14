namespace AgeRegression.Items;

/// <summary>
/// Minimal context for item acquisition requests.
/// </summary>
public sealed record AcquisitionContext(
    AcquisitionSource Source = AcquisitionSource.Console,
    bool HasBooster = false,
    int CurrentAbsoluteDay = 0);
