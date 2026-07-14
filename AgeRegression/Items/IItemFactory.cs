using StardewObject = StardewValley.Object;

namespace AgeRegression.Items;

/// <summary>
/// Abstraction for creating runtime wardrobe items.
/// </summary>
public interface IItemFactory
{
    StardewObject? CreateFromResolved(
        ResolvedWardrobeItem resolved,
        bool hasBooster = false,
        int currentAbsoluteDay = 0);
}
