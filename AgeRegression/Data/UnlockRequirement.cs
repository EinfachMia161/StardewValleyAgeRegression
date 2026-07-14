using System.Collections.Generic;

namespace AgeRegression.Data;

/// <summary>
/// Container for the unlock conditions attached to a wardrobe item. It is
/// embedded in item definitions and consumed by
/// <see cref="Items.ItemUnlockService"/>.
///
/// <para>
/// All listed <see cref="Conditions"/> must be met (logical AND). An empty
/// or missing <see cref="Conditions"/> list means the item is always
/// unlocked.
/// </para>
/// </summary>
public sealed class UnlockRequirement
{
    /// <summary>
    /// Explicit, composable list of unlock conditions. Every entry must be
    /// satisfied for the item to unlock. When <c>null</c> or empty the item
    /// has no unlock requirement.
    /// </summary>
    public List<IUnlockCondition>? Conditions { get; set; }

    /// <summary>
    /// Returns the conditions that apply to this requirement. An empty list
    /// (or a <c>null</c> <see cref="Conditions"/>) means the item is always
    /// unlocked.
    /// </summary>
    public IEnumerable<IUnlockCondition> GetConditions() =>
        Conditions ?? new List<IUnlockCondition>();
}
