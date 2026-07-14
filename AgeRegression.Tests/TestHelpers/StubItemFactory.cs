using AgeRegression.Items;
using StardewObject = StardewValley.Object;

namespace AgeRegression.Tests;

internal sealed class StubItemFactory : IItemFactory
{
    public StardewObject? CreatedItem { get; private set; }

    public StardewObject? CreateFromResolved(
        ResolvedWardrobeItem resolved,
        bool hasBooster = false,
        int currentAbsoluteDay = 0)
    {
        if (resolved.ItemId == "bad_diaper")
        {
            CreatedItem = null;
            return null;
        }

        CreatedItem = new StardewObject();
        return CreatedItem;
    }
}
