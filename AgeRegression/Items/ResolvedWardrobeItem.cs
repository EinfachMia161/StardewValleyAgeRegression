using AgeRegression.Data;

namespace AgeRegression.Items;

/// <summary>
/// The result of resolving a wardrobe item ID.
/// Contains category, definition, and sprite metadata.
/// Does not hold any Stardew runtime objects.
/// </summary>
public sealed class ResolvedWardrobeItem
{
    public WardrobeCategory Category { get; }

    /// <summary>The item's definition. Never null.</summary>
    public IWardrobeItemDefinition Definition { get; }

    /// <summary>Non-null when <see cref="Category"/> is <see cref="WardrobeCategory.Diaper"/>.</summary>
    public DiaperTypeData? DiaperDefinition => Definition as DiaperTypeData;

    /// <summary>Non-null when <see cref="Category"/> is <see cref="WardrobeCategory.Accessory"/>.</summary>
    public WardrobeItemData? AccessoryDefinition => Definition as WardrobeItemData;

    public string ItemId      => Definition.Id;
    public string SpriteSheet => Definition.SpriteSheet;
    public int    SpriteIndex => Definition.SpriteIndex;

    private ResolvedWardrobeItem(WardrobeCategory category, IWardrobeItemDefinition definition)
    {
        Category   = category;
        Definition = definition;
    }

    internal static ResolvedWardrobeItem FromDiaper(DiaperTypeData def) =>
        new(WardrobeCategory.Diaper, def);

    internal static ResolvedWardrobeItem FromAccessory(WardrobeItemData def) =>
        new(WardrobeCategory.Accessory, def);
}
