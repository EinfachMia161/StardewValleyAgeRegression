namespace AgeRegression.State;

/// <summary>
/// Full regression state for a player save.
/// </summary>
public sealed class PlayerRegressionState
{
    public long PlayerId { get; set; }

    public string CurrentStageId { get; set; } = "none";

    public DiaperState Diaper { get; set; } = DiaperState.None;

    public ComfortState Comfort { get; set; } = new();

    public PlayerNeedsState Needs { get; set; } = new();

    public MoodState Mood { get; set; } = new();

    public HashSet<string> EquippedAccessories { get; set; } = new();

    public Dictionary<string, int> DialogueCooldowns { get; set; } = new();

    public int AccidentsToday { get; set; }

    public int LastUpdatedAbsoluteDay { get; set; }

    public int SpouseDailyDialogueLastGivenAbsoluteDay { get; set; }
}
