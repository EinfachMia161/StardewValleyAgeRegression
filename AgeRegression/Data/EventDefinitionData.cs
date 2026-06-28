namespace AgeRegression.Data;

/// <summary>
/// Defines a custom game event (cutscene) that can be triggered by
/// the mod's event system.
///
/// <para>
/// Event scripts use Stardew Valley's native event command format.
/// They are stored in <c>assets/events/</c> as plain text files and
/// injected into <c>Data/Events/{LocationName}</c> via
/// <c>AssetRequested</c>.
/// </para>
///
/// <para>
/// Preconditions use SV's native precondition format (e.g.
/// <c>"f Penny 2000"</c>) combined with mod-specific preconditions
/// evaluated by
/// <see cref="AgeRegression.Events.EventConditionEvaluator"/>.
/// </para>
/// </summary>
public sealed class EventDefinitionData
{
    /// <summary>
    /// Unique event ID. Used as the key in
    /// <c>Data/Events/{Location}</c>.
    /// Must be unique across all mods.
    /// Convention: <c>mia.AgeRegression.{Name}</c>.
    /// </summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// The game location where this event triggers.
    /// Example: <c>"FarmHouse"</c>, <c>"Town"</c>, <c>"Forest"</c>.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Path to the event script file relative to the mod root.
    /// Example: <c>assets/events/first_regression.txt</c>.
    /// </summary>
    public string ScriptFile { get; set; } = string.Empty;

    /// <summary>
    /// Stardew Valley native preconditions string.
    /// Example: <c>"f Penny 2000/t 600 1200"</c>.
    /// <c>null</c> means no native preconditions.
    /// </summary>
    public string? NativePreconditions { get; set; }

    /// <summary>
    /// Mod-specific conditions that must be satisfied before this event
    /// can trigger. Uses the same <see cref="DialogueConditions"/> model
    /// as dialogue selection.
    /// </summary>
    public DialogueConditions? ModConditions { get; set; }

    /// <summary>
    /// Whether this event can only trigger once per save file.
    /// Most story events should be true. Repeatable comfort events false.
    /// </summary>
    public bool OneTimeOnly { get; set; } = true;

    /// <summary>
    /// Minimum number of in-game days between triggers for repeatable
    /// events. Ignored when <see cref="OneTimeOnly"/> is true.
    /// </summary>
    public int RepeatCooldownDays { get; set; } = 7;

    /// <summary>
    /// Priority order when multiple events are eligible simultaneously.
    /// Higher values trigger first.
    /// </summary>
    public int Priority { get; set; } = 0;
}
