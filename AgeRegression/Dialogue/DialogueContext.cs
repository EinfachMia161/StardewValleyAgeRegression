using AgeRegression.State;

namespace AgeRegression.Dialogue;

/// <summary>
/// A snapshot of all context needed to evaluate dialogue conditions.
/// Passed to <see cref="DialogueConditionEvaluator"/> as a single
/// parameter so the evaluator has no dependencies on live game state.
/// This makes condition evaluation fully unit-testable.
/// </summary>
public sealed class DialogueContext
{
    // -------------------------------------------------------------------------
    // Player state
    // -------------------------------------------------------------------------

    /// <summary>Current regression stage ID.</summary>
    public string RegressionStageId { get; init; } = "none";

    /// <summary>Current regression stage order (0 = baseline).</summary>
    public int RegressionStageOrder { get; init; } = 0;

    /// <summary>
    /// Current diaper condition ID
    /// (clean/damp/wet/soaked/none).
    /// </summary>
    public string DiaperConditionId { get; init; } = "none";

    /// <summary>Whether the player is wearing a diaper.</summary>
    public bool IsWearingDiaper { get; init; } = false;

    /// <summary>Normalized continence value (0–1).</summary>
    public float ContinenceNormalized { get; init; } = 1f;

    /// <summary>Current continence threshold ID.</summary>
    public string ContinenceThresholdId { get; init; } = string.Empty;

    /// <summary>Normalized hunger value (0–1).</summary>
    public float HungerNormalized { get; init; } = 1f;

    /// <summary>Normalized thirst value (0–1).</summary>
    public float ThirstNormalized { get; init; } = 1f;

    /// <summary>Normalized comfort value (0–1).</summary>
    public float ComfortNormalized { get; init; } = 1f;

    /// <summary>Currently equipped accessory IDs.</summary>
    public IReadOnlySet<string> EquippedAccessories { get; init; } =
        new HashSet<string>();

    // -------------------------------------------------------------------------
    // NPC context
    // -------------------------------------------------------------------------

    /// <summary>The NPC's name.</summary>
    public string NpcName { get; init; } = string.Empty;

    /// <summary>Friendship hearts with this NPC (0–14).</summary>
    public int FriendshipHearts { get; init; } = 0;

    /// <summary>Whether the player is married to this NPC.</summary>
    public bool IsMarried { get; init; } = false;

    /// <summary>Personality tags on this NPC's profile.</summary>
    public IReadOnlyList<string> NpcPersonalityTags { get; init; } =
        Array.Empty<string>();

    // -------------------------------------------------------------------------
    // World context
    // -------------------------------------------------------------------------

    /// <summary>
    /// Current season (spring/summer/fall/winter).
    /// </summary>
    public string Season { get; init; } = string.Empty;

    /// <summary>
    /// Current in-game time (e.g. 630, 1200).
    /// </summary>
    public int TimeOfDay { get; init; } = 600;

    /// <summary>Current location name.</summary>
    public string LocationName { get; init; } = string.Empty;

    /// <summary>
    /// Current weather ID (sunny/rainy/stormy/snowy/windy).
    /// </summary>
    public string Weather { get; init; } = "sunny";

    /// <summary>Whether a festival is active today.</summary>
    public bool IsFestivalDay { get; init; } = false;

    /// <summary>
    /// Game state flags (mail received, events seen, etc.).
    /// Mail flags are stored as-is. Event IDs are prefixed with
    /// <c>event_</c> to avoid collisions with mail flag names.
    /// </summary>
    public IReadOnlySet<string> GameFlags { get; init; } =
        new HashSet<string>();

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds a <see cref="DialogueContext"/> from live game state.
    /// Call only when <c>Context.IsWorldReady</c>.
    /// </summary>
    public static DialogueContext FromGameState(
        PlayerRegressionState playerState,
        Data.RegressionStageData currentStage,
        Data.NpcReactionProfileData? npcProfile,
        StardewValley.NPC npc,
        float maxComfort)
    {
        var farmer     = StardewValley.Game1.player;
        var friendship = farmer.getFriendshipHeartLevelForNPC(npc.Name);
        var isMarried  = !string.IsNullOrEmpty(farmer.spouse) &&
            string.Equals(farmer.spouse, npc.Name,
                StringComparison.OrdinalIgnoreCase);

        var weather = StardewValley.Game1.isLightning ? "stormy"
            : StardewValley.Game1.isRaining           ? "rainy"
            : StardewValley.Game1.isSnowing           ? "snowy"
            : StardewValley.Game1.isDebrisWeather     ? "windy"
            : "sunny";

        var continenceThreshold =
            playerState.Needs.Continence.Value.LastKnownThresholdId;

        var gameFlags = new HashSet<string>(
            StardewValley.Game1.player.mailReceived
                .Concat(StardewValley.Game1.player.eventsSeen
                    .Select(id => $"event_{id}")));

        return new DialogueContext
        {
            RegressionStageId    = currentStage.Id,
            RegressionStageOrder = currentStage.Order,
            DiaperConditionId    = playerState.Diaper.IsWearingDiaper
                ? playerState.Diaper.ConditionId : "none",
            IsWearingDiaper      = playerState.Diaper.IsWearingDiaper,
            ContinenceNormalized = playerState.Needs.Continence.Value.Normalized,
            ContinenceThresholdId = continenceThreshold,
            HungerNormalized     = playerState.Needs.Hunger.Normalized,
            ThirstNormalized     = playerState.Needs.Thirst.Normalized,
            ComfortNormalized    = playerState.Comfort.GetNormalized(maxComfort),
            EquippedAccessories  = playerState.EquippedAccessories,
            NpcName              = npc.Name,
            FriendshipHearts     = friendship,
            IsMarried            = isMarried,
            NpcPersonalityTags   = npcProfile?.PersonalityTags
                ?? (IReadOnlyList<string>)Array.Empty<string>(),
            Season               = StardewValley.Game1.currentSeason,
            TimeOfDay            = StardewValley.Game1.timeOfDay,
            LocationName         = StardewValley.Game1.currentLocation?.Name
                ?? string.Empty,
            Weather              = weather,
            IsFestivalDay        = StardewValley.Utility.isFestivalDay(
                StardewValley.Game1.dayOfMonth,
                StardewValley.Game1.season),
            GameFlags            = gameFlags
        };
    }
}
