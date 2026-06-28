namespace AgeRegression.Data;

/// <summary>
/// Defines a diaper type loaded from <c>assets/data/diaper-types.json</c>.
///
/// <para>
/// All gameplay values are stored here. No diaper behavior is hardcoded
/// in C#. Systems read these values and apply them through the existing
/// modifier provider and event architectures.
/// </para>
/// </summary>
public sealed class DiaperTypeData
{
    // -------------------------------------------------------------------------
    // Identity
    // -------------------------------------------------------------------------

    /// <summary>
    /// Unique identifier. Used to construct the qualified item ID
    /// <c>(O)mia.AgeRegression_Diaper_{Id}</c>.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Display name shown in inventory.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Item description shown in tooltip.</summary>
    public string Description { get; set; } = string.Empty;

    // -------------------------------------------------------------------------
    // Capacity and absorption
    // -------------------------------------------------------------------------

    /// <summary>
    /// Maximum wetness capacity in arbitrary units.
    /// Higher values mean the diaper holds more before reaching soaked state.
    /// </summary>
    public float MaxCapacity { get; set; } = 100f;

    /// <summary>
    /// Absorption rate multiplier (0.0–2.0).
    /// Values above 1.0 mean the diaper absorbs wetness more efficiently,
    /// reducing the visible wetness level increase per accident.
    /// Values below 1.0 mean the diaper absorbs less efficiently.
    /// Default 1.0 = standard absorption.
    /// </summary>
    public float AbsorptionRate { get; set; } = 1.0f;

    // -------------------------------------------------------------------------
    // Physical properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Thickness rating (1–5). Higher values may trigger NPC comments
    /// and affect movement speed via <see cref="ThicknessSpeedPenalty"/>.
    /// </summary>
    public int Thickness { get; set; } = 2;

    /// <summary>
    /// Padding rating (1–5). Influences comfort bonus and capacity.
    /// </summary>
    public int Padding { get; set; } = 2;

    /// <summary>
    /// Speed multiplier applied while wearing this diaper due to bulk.
    /// 1.0 = no effect. 0.95 = 5% speed reduction from thickness.
    /// Stacks with regression stage and mood modifiers via
    /// <see cref="Items.ItemStatModifierProvider"/>.
    /// </summary>
    public float ThicknessSpeedPenalty { get; set; } = 1.0f;

    /// <summary>Whether this diaper type supports booster inserts.</summary>
    public bool SupportsBooster { get; set; } = false;

    // -------------------------------------------------------------------------
    // Comfort effects
    // -------------------------------------------------------------------------

    /// <summary>
    /// Comfort bonus applied immediately when this diaper is equipped clean.
    /// Represents the security and comfort of putting on a fresh diaper.
    /// Published via <see cref="Events.DiaperComfortEffectEventArgs"/>.
    /// </summary>
    public float EquipComfortBonus { get; set; } = 5f;

    /// <summary>
    /// Multiplier applied to <see cref="EquipComfortBonus"/> when computing
    /// the comfort bonus for a diaper change (as opposed to a first equip).
    /// Range: 0.0–1.0. Default 0.8 = 80% of the equip bonus.
    ///
    /// <para>
    /// Rationale: a diaper change provides comfort but slightly less than
    /// putting on a fresh diaper for the first time. This value allows
    /// per-type tuning — a premium diaper might have a higher ratio because
    /// the change experience is more comfortable.
    /// </para>
    /// </summary>
    public float ChangeComfortBonusRatio { get; set; } = 0.8f;

    /// <summary>
    /// Comfort delta applied when this diaper is removed.
    /// Negative values represent the loss of security on removal.
    /// Published via <see cref="Events.DiaperComfortEffectEventArgs"/>.
    /// </summary>
    public float RemoveComfortDelta { get; set; } = -3f;

    /// <summary>
    /// Comfort bonus applied per in-game hour while wearing this diaper clean.
    /// Used by the comfort modifier system via
    /// <c>assets/data/comfort-modifiers.json</c>.
    /// </summary>
    public float CleanComfortPerHour { get; set; } = 3f;

    /// <summary>
    /// Comfort penalty applied per in-game hour when the diaper is saturated.
    /// </summary>
    public float SaturatedComfortPenalty { get; set; } = 20f;

    // -------------------------------------------------------------------------
    // Continence influence
    // -------------------------------------------------------------------------

    /// <summary>
    /// Multiplier applied to the continence drain rate while wearing this
    /// diaper. Values below 1.0 reduce drain (the security of wearing a
    /// diaper slows continence loss). Values above 1.0 increase drain.
    /// Range: 0.5–1.5. Default 0.9 = 10% reduction in drain rate.
    /// </summary>
    public float ContinenceDrainMultiplier { get; set; } = 0.9f;

    // -------------------------------------------------------------------------
    // Item metadata
    // -------------------------------------------------------------------------

    /// <summary>Sprite index within the mod's diaper sprite sheet.</summary>
    public int SpriteIndex { get; set; } = 0;

    /// <summary>Sell price in gold.</summary>
    public int Price { get; set; } = 50;
}
