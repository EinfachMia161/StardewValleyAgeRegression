using AgeRegression.Items;

namespace AgeRegression.Tests;

/// <summary>
/// Reusable test helper exposing an always-unlocked
/// <see cref="ItemUnlockService"/>, for tests that focus on stock/price
/// logic rather than unlock gating.
/// </summary>
internal static class TestItemUnlockService
{
    public static ItemUnlockService AlwaysUnlocked { get; } =
        new(new AlwaysUnlockedGameState());
}
