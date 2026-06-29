namespace AgeRegression.Events;

/// <summary>
/// Published when an accessory is equipped or unequipped.
/// Used by ComfortSystem to apply immediate comfort effects.
/// </summary>
public sealed class AccessoryChangedEventArgs
{
    /// <summary>The accessory ID that changed.</summary>
    public string AccessoryId { get; }

    /// <summary>Whether the accessory was equipped (true) or unequipped (false).</summary>
    public bool Equipped { get; }

    /// <summary>
    /// The unique ID of the player whose accessory changed.
    /// </summary>
    public long PlayerId { get; }

    public AccessoryChangedEventArgs(
        string accessoryId,
        bool equipped,
        long playerId)
    {
        AccessoryId = accessoryId;
        Equipped = equipped;
        PlayerId = playerId;
    }
}