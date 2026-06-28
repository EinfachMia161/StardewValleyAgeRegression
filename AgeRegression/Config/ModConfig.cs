namespace AgeRegression.Config;

/// <summary>
/// Root configuration object. Serialized to and from <c>config.json</c> by SMAPI.
/// Each nested object represents a logical feature area so the file stays
/// organized as the mod grows.
/// </summary>
public sealed class ModConfig
{
    /// <summary>
    /// Master switch. When <c>false</c>, all mod systems are disabled
    /// and no game state is modified.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Minimum log level written to the SMAPI console.
    /// Valid values: Trace, Debug, Info, Warn, Error.
    /// </summary>
    public string LogLevel { get; set; } = "Debug";

    /// <summary>Settings governing regression stage transitions and stat effects.</summary>
    public RegressionConfig Regression { get; set; } = new();

    /// <summary>Settings governing diaper mechanics.</summary>
    public DiaperConfig Diapers { get; set; } = new();

    /// <summary>Settings governing the comfort system.</summary>
    public ComfortConfig Comfort { get; set; } = new();

    /// <summary>Settings governing NPC and spouse interactions.</summary>
    public NpcConfig Npcs { get; set; } = new();

    /// <summary>Settings governing the needs systems.</summary>
    public NeedsConfig Needs { get; set; } = new();

    /// <summary>Settings governing player notifications.</summary>
    public NotificationConfig Notifications { get; set; } = new();

    /// <summary>Settings governing the in-game status HUD overlay.</summary>
    public HudConfig Hud { get; set; } = new();
}
