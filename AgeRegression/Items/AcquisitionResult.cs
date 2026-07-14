using StardewObject = StardewValley.Object;

namespace AgeRegression.Items;

/// <summary>
/// Result of an acquisition attempt.
/// </summary>
public sealed class AcquisitionResult
{
    public bool Success { get; }
    public AcquisitionFailureReason FailureReason { get; }
    public ResolvedWardrobeItem? ResolvedItem { get; }
    public StardewObject? CreatedItem { get; }

    private AcquisitionResult(
        bool success,
        AcquisitionFailureReason failureReason,
        ResolvedWardrobeItem? resolvedItem,
        StardewObject? createdItem)
    {
        Success = success;
        FailureReason = failureReason;
        ResolvedItem = resolvedItem;
        CreatedItem = createdItem;
    }

    public static AcquisitionResult Succeeded(
        ResolvedWardrobeItem resolvedItem,
        StardewObject createdItem) =>
        new(true, AcquisitionFailureReason.None, resolvedItem, createdItem);

    public static AcquisitionResult Failed(AcquisitionFailureReason reason) =>
        new(false, reason, null, null);
}

/// <summary>
/// Describes why an acquisition attempt failed.
/// </summary>
public enum AcquisitionFailureReason
{
    None,
    UnknownItem,
    Locked,
    CreationFailed
}
