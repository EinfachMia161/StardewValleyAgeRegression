namespace AgeRegression.State;

/// <summary>
/// Snapshot of the player's diaper state.
/// </summary>
public sealed class DiaperState
{
    public string? EquippedDiaperTypeId { get; set; }

    public float WetnessLevel { get; set; }

    public float MessingLevel { get; set; }

    public bool HasBooster { get; set; }

    public int LastChangedAbsoluteDay { get; set; }

    public bool IsWearingDiaper => !string.IsNullOrWhiteSpace(EquippedDiaperTypeId);

    public string ConditionId
    {
        get
        {
            if (!IsWearingDiaper) return "none";
            if (MessingLevel >= 0.66f || WetnessLevel >= 0.66f) return "soaked";
            if (MessingLevel >= 0.33f || WetnessLevel >= 0.33f) return "wet";
            if (MessingLevel > 0f || WetnessLevel > 0f) return "damp";
            return "clean";
        }
    }

    public static DiaperState None { get; } = new();

    public static DiaperState CreateFresh(
        string? diaperTypeId,
        bool hasBooster = false,
        int lastChangedAbsoluteDay = 0)
    {
        return new DiaperState
        {
            EquippedDiaperTypeId = diaperTypeId,
            WetnessLevel = 0f,
            MessingLevel = 0f,
            HasBooster = hasBooster,
            LastChangedAbsoluteDay = lastChangedAbsoluteDay
        };
    }

    // Value equality — compared in StateManager.UpdateDiaperState to suppress spurious events.
    public bool Equals(DiaperState? other)
    {
        if (other is null) return false;
        return EquippedDiaperTypeId == other.EquippedDiaperTypeId
            && Math.Abs(WetnessLevel - other.WetnessLevel) < 0.001f
            && Math.Abs(MessingLevel - other.MessingLevel) < 0.001f
            && HasBooster == other.HasBooster
            && LastChangedAbsoluteDay == other.LastChangedAbsoluteDay;
    }

    public override bool Equals(object? obj) => Equals(obj as DiaperState);

    public override int GetHashCode() =>
        HashCode.Combine(EquippedDiaperTypeId, WetnessLevel, MessingLevel,
            HasBooster, LastChangedAbsoluteDay);

    public static bool operator ==(DiaperState? a, DiaperState? b) =>
        a is null ? b is null : a.Equals(b);

    public static bool operator !=(DiaperState? a, DiaperState? b) => !(a == b);

    public DiaperState WithWetness(float wetness) => new()
    {
        EquippedDiaperTypeId = EquippedDiaperTypeId,
        WetnessLevel = Math.Clamp(wetness, 0f, 1f),
        MessingLevel = MessingLevel,
        HasBooster = HasBooster,
        LastChangedAbsoluteDay = LastChangedAbsoluteDay
    };

    public DiaperState WithMessing(float messing) => new()
    {
        EquippedDiaperTypeId = EquippedDiaperTypeId,
        WetnessLevel = WetnessLevel,
        MessingLevel = messing,
        HasBooster = HasBooster,
        LastChangedAbsoluteDay = LastChangedAbsoluteDay
    };
}
