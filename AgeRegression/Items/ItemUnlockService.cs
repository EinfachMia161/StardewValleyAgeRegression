namespace AgeRegression.Items;

/// <summary>
/// Public entry point for deciding whether a wardrobe item is unlocked.
///
/// <para>
/// This is a thin orchestration layer: it retrieves the item's conditions,
/// evaluates each one through <see cref="UnlockConditionEvaluator"/>, and
/// combines the results with a logical AND. It contains no Stardew API
/// calls, no game-state lookups, no condition-specific logic, and no
/// item, shop, or rendering logic — all interpretation lives in
/// <see cref="UnlockConditionEvaluator"/>.
/// </para>
/// </summary>
/// <remarks>
/// Pipeline:
/// <see cref="ResolvedWardrobeItem"/> → <see cref="ItemUnlockService"/>
/// → <see cref="UnlockRequirement.GetConditions"/> →
/// <see cref="UnlockConditionEvaluator"/> → bool IsUnlocked
///
/// Reusable by shops, crafting, mail rewards, NPC gifts, quest rewards,
/// and loot tables without modification.
/// </remarks>
public sealed class ItemUnlockService
{
    private readonly UnlockConditionEvaluator _evaluator;

    public ItemUnlockService(IGameStateProvider gameState)
    {
        _evaluator = new UnlockConditionEvaluator(gameState);
    }

    /// <summary>
    /// Returns <c>true</c> if every one of the item's unlock conditions is
    /// currently met. An item with no conditions is always unlocked.
    /// </summary>
    /// <remarks>
    /// Conditions are obtained from <see cref="UnlockRequirement.GetConditions"/>,
    /// each is evaluated through <see cref="UnlockConditionEvaluator"/>, and
    /// the results are combined with a logical AND — returning <c>false</c>
    /// as soon as any single condition fails.
    /// </remarks>
    public bool IsUnlocked(ResolvedWardrobeItem item)
    {
        foreach (var condition in item.Definition.Unlock.GetConditions())
        {
            if (!_evaluator.Evaluate(condition))
                return false;
        }

        return true;
    }
}
