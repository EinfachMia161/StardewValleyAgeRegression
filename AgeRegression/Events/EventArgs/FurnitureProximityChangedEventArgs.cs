namespace AgeRegression.Events;

/// <summary>
/// Published by <see cref="Systems.FurnitureProximitySystem"/> when the
/// set of comfort-providing furniture within proximity range changes.
///
/// <para>
/// <see cref="Systems.ComfortSystem"/> subscribes to this event to
/// recalculate active comfort modifiers when the player moves near or
/// away from furniture. Using a dedicated event type avoids the semantic
/// misuse of <see cref="NeedsValueChangedEventArgs"/> as a generic
/// refresh trigger.
/// </para>
/// </summary>
public sealed class FurnitureProximityChangedEventArgs
{
    /// <summary>
    /// Comfort modifier IDs that became active in this update
    /// (furniture the player just moved near).
    /// </summary>
    public IReadOnlyList<string> AddedModifierIds { get; }

    /// <summary>
    /// Comfort modifier IDs that became inactive in this update
    /// (furniture the player just moved away from).
    /// </summary>
    public IReadOnlyList<string> RemovedModifierIds { get; }

    /// <summary>
    /// The full set of comfort modifier IDs currently active after
    /// this update.
    /// </summary>
    public IReadOnlySet<string> CurrentActiveModifierIds { get; }

    /// <summary>
    /// The unique ID of the player whose proximity changed.
    /// </summary>
    public long PlayerId { get; }

    public FurnitureProximityChangedEventArgs(
        IReadOnlyList<string> addedModifierIds,
        IReadOnlyList<string> removedModifierIds,
        IReadOnlySet<string> currentActiveModifierIds,
        long playerId)
    {
        AddedModifierIds         = addedModifierIds;
        RemovedModifierIds       = removedModifierIds;
        CurrentActiveModifierIds = currentActiveModifierIds;
        PlayerId                 = playerId;
    }
}
