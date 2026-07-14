namespace AgeRegression.Data;

/// <summary>
/// Defines a wearable accessory item loaded from
/// <c>assets/data/wardrobe-items.json</c>.
/// </summary>
public sealed class WardrobeItemData : IWardrobeItemDefinition
{
    /// <summary>
    /// Unique identifier. Used as the accessory slot key and to construct
    /// the qualified item ID
    /// <c>(O)mia.AgeRegression_Accessory_{Id}</c>.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Display name shown in inventory.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Item description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The slot category this item occupies.
    /// Only one item per slot can be equipped at a time.
    /// Example values: <c>"pacifier"</c>, <c>"bottle"</c>,
    /// <c>"mittens"</c>, <c>"bib"</c>, <c>"onesie"</c>,
    /// <c>"plushie"</c>.
    /// </summary>
    public string Slot { get; set; } = string.Empty;

    /// <summary>
    /// Immediate comfort bonus applied when this item is equipped.
    ///
    /// <para>
    /// <b>Current behavior (symmetric):</b> When the item is unequipped,
    /// the negative of this value is applied as a comfort penalty.
    /// Equipping gives +<see cref="ComfortBonus"/>; unequipping gives
    /// -<see cref="ComfortBonus"/>. Net effect over an equip/unequip
    /// cycle is zero.
    /// </para>
    ///
    /// <para>
    /// <b>Future tuning option:</b> A separate
    /// <c>UnequipComfortPenalty</c> field (float, default equal to
    /// <see cref="ComfortBonus"/>) would allow asymmetric behavior.
    /// For example, a pacifier might give +8 comfort on equip but only
    /// -4 on unequip, reflecting that the player retains some residual
    /// comfort after putting it down. This field is not implemented yet
    /// to keep the data model simple. Add it when per-item unequip
    /// tuning becomes a gameplay requirement.
    /// </para>
    /// </summary>
    public float ComfortBonus { get; set; } = 0f;

    /// <summary>
    /// Speed multiplier modifier while this item is equipped.
    /// Stacks multiplicatively with regression stage and mood modifiers
    /// via <see cref="Items.ItemStatModifierProvider"/>.
    /// 1.0 = no effect.
    /// </summary>
    public float SpeedModifier { get; set; } = 1.0f;

    /// <summary>
    /// Whether equipping this item requires the player to be at a
    /// minimum regression stage order. 0 = no requirement.
    /// </summary>
    public int MinStageOrder { get; set; } = 0;

    /// <summary>Sell price in gold.</summary>
    public int Price { get; set; } = 25;

    /// <summary>
    /// Whether this item appears in shops.
    /// Defaults to <c>true</c> so new items are automatically available.
    /// </summary>
    public bool ShopAvailable { get; set; } = true;

    /// <summary>
    /// Unlock condition for this item.
    /// Absent from JSON → empty <see cref="UnlockRequirement"/> → always unlocked.
    /// Evaluated by <see cref="Items.ItemUnlockService"/>.
    /// </summary>
    public UnlockRequirement Unlock { get; set; } = new();

    /// <summary>Sprite index within the sheet (0-based).</summary>
    public int SpriteIndex { get; set; } = 0;

    /// <summary>
    /// Sprite sheet path relative to mod root.
    /// </summary>
    public string SpriteSheet { get; set; } = "assets/placeholder/sprites.png";
}
