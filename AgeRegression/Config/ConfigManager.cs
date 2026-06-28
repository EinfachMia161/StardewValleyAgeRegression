using AgeRegression.Utilities;
using StardewModdingAPI;

namespace AgeRegression.Config;

/// <summary>
/// Instance-based service that manages configuration lifecycle:
/// validation on load, GMCM registration, and writing changes back to disk.
///
/// <para>
/// Systems that need configuration receive <see cref="ModConfig"/> directly
/// rather than depending on this manager. This class is owned by
/// <see cref="ModEntry"/> and is not passed to other systems.
/// </para>
/// </summary>
public sealed class ConfigManager
{
    private ModConfig _config;
    private readonly IModHelper _helper;
    private readonly LogHelper _log;

    /// <param name="config">The config loaded by SMAPI at startup.</param>
    /// <param name="helper">The SMAPI mod helper.</param>
    /// <param name="log">The mod logger.</param>
    public ConfigManager(ModConfig config, IModHelper helper, LogHelper log)
    {
        _config = config;
        _helper = helper;
        _log    = log;

        Validate();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// The current configuration. Always valid — <see cref="Validate"/> has run.
    /// </summary>
    public ModConfig Config => _config;

    /// <summary>
    /// Registers all config options with Generic Mod Config Menu if installed.
    /// Safe to call when GMCM is absent.
    /// </summary>
    public void RegisterWithGmcm()
    {
        var gmcmApi = _helper.ModRegistry.GetApi<IGmcmApi>(
            "spacechase0.GenericModConfigMenu");

        if (gmcmApi is null)
        {
            _log.Debug("GMCM not installed — skipping config menu registration.");
            return;
        }

        var manifest = GetManifest();
        if (manifest is null)
        {
            _log.Warn("Could not retrieve mod manifest for GMCM registration.");
            return;
        }

        gmcmApi.Register(
            mod: manifest,
            reset: () =>
            {
                _config = new ModConfig();
                Validate();
            },
            save: () =>
            {
                Validate();
                _helper.WriteConfig(_config);
            });

        RegisterGmcmOptions(gmcmApi, manifest);

        _log.Debug("GMCM registration complete.");
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private StardewModdingAPI.IManifest? GetManifest()
    {
        return _helper.ModRegistry
            .Get(_helper.ModContent.ModID)
            ?.Manifest;
    }

    private void RegisterGmcmOptions(IGmcmApi gmcmApi, StardewModdingAPI.IManifest manifest)
    {
        // --- General ---
        gmcmApi.AddSectionTitle(manifest, () => "General");

        gmcmApi.AddBoolOption(
            mod: manifest,
            name: () => "Mod Enabled",
            tooltip: () => "Master switch. Disabling this turns off all mod systems.",
            getValue: () => _config.Enabled,
            setValue: v => _config.Enabled = v);

        // --- Regression ---
        gmcmApi.AddSectionTitle(manifest, () => "Regression");

        gmcmApi.AddBoolOption(
            mod: manifest,
            name: () => "Stat Modifiers",
            tooltip: () => "Whether regression stages affect player stats.",
            getValue: () => _config.Regression.StatModifiersEnabled,
            setValue: v => _config.Regression.StatModifiersEnabled = v);

        gmcmApi.AddBoolOption(
            mod: manifest,
            name: () => "Allow Manual Regression",
            tooltip: () => "Whether the player can manually change their regression stage.",
            getValue: () => _config.Regression.AllowManualRegression,
            setValue: v => _config.Regression.AllowManualRegression = v);

        // --- Diapers ---
        gmcmApi.AddSectionTitle(manifest, () => "Diapers");

        gmcmApi.AddBoolOption(
            mod: manifest,
            name: () => "Wetness System",
            tooltip: () => "Enables diaper wetness tracking.",
            getValue: () => _config.Diapers.WetnessEnabled,
            setValue: v => _config.Diapers.WetnessEnabled = v);

        gmcmApi.AddBoolOption(
            mod: manifest,
            name: () => "Messing System",
            tooltip: () => "Enables optional messing mechanics.",
            getValue: () => _config.Diapers.MessingEnabled,
            setValue: v => _config.Diapers.MessingEnabled = v);

        gmcmApi.AddBoolOption(
            mod: manifest,
            name: () => "Accident Mechanics",
            tooltip: () => "Enables accident mechanics.",
            getValue: () => _config.Diapers.AccidentsEnabled,
            setValue: v => _config.Diapers.AccidentsEnabled = v);

        gmcmApi.AddNumberOption(
            mod: manifest,
            name: () => "Wetness Tick Interval (minutes)",
            tooltip: () => "How many in-game minutes between wetness checks. Minimum 10.",
            getValue: () => _config.Diapers.WetnessTickIntervalMinutes,
            setValue: v => _config.Diapers.WetnessTickIntervalMinutes = v,
            min: 10,
            max: 120,
            interval: 10);

        // --- Comfort ---
        gmcmApi.AddSectionTitle(manifest, () => "Comfort");

        gmcmApi.AddBoolOption(
            mod: manifest,
            name: () => "Comfort System",
            tooltip: () => "Enables the comfort and mood system.",
            getValue: () => _config.Comfort.Enabled,
            setValue: v => _config.Comfort.Enabled = v);

        gmcmApi.AddNumberOption(
            mod: manifest,
            name: () => "Passive Comfort Decay (per hour)",
            tooltip: () => "Global comfort decay scalar per in-game hour. Set to 0 to disable.",
            getValue: () => _config.Comfort.PassiveDecayPerHour,
            setValue: v => _config.Comfort.PassiveDecayPerHour = v,
            min: 0f,
            max: 20f,
            interval: 0.5f);

        // --- NPCs ---
        gmcmApi.AddSectionTitle(manifest, () => "NPCs");

        gmcmApi.AddBoolOption(
            mod: manifest,
            name: () => "NPC Reactions",
            tooltip: () => "Whether NPCs react to the player's regression state.",
            getValue: () => _config.Npcs.NpcReactionsEnabled,
            setValue: v => _config.Npcs.NpcReactionsEnabled = v);

        gmcmApi.AddBoolOption(
            mod: manifest,
            name: () => "Spouse Interactions",
            tooltip: () => "Enables extended spouse interaction content.",
            getValue: () => _config.Npcs.SpouseInteractionsEnabled,
            setValue: v => _config.Npcs.SpouseInteractionsEnabled = v);

        gmcmApi.AddNumberOption(
            mod: manifest,
            name: () => "Min Friendship for NPC Reactions (hearts)",
            tooltip: () => "Minimum friendship hearts before an NPC reacts to regression.",
            getValue: () => _config.Npcs.MinFriendshipForReaction,
            setValue: v => _config.Npcs.MinFriendshipForReaction = v,
            min: 0,
            max: 14,
            interval: 1);

        gmcmApi.AddNumberOption(
            mod: manifest,
            name: () => "Dialogue Cooldown (days)",
            tooltip: () => "Days before an NPC repeats the same regression dialogue.",
            getValue: () => _config.Npcs.DialogueCooldownDays,
            setValue: v => _config.Npcs.DialogueCooldownDays = v,
            min: 0,
            max: 14,
            interval: 1);

        // --- Needs ---
        gmcmApi.AddSectionTitle(manifest, () => "Needs");

        gmcmApi.AddBoolOption(
            mod: manifest,
            name: () => "Needs System",
            tooltip: () => "Enables the continence, hunger, and thirst systems.",
            getValue: () => _config.Needs.Enabled,
            setValue: v => _config.Needs.Enabled = v);

        gmcmApi.AddBoolOption(
            mod: manifest,
            name: () => "Continence System",
            tooltip: () => "Enables the continence hidden stat.",
            getValue: () => _config.Needs.Continence.Enabled,
            setValue: v => _config.Needs.Continence.Enabled = v);

        gmcmApi.AddBoolOption(
            mod: manifest,
            name: () => "Hunger System",
            tooltip: () => "Enables the hunger hidden stat.",
            getValue: () => _config.Needs.Hunger.Enabled,
            setValue: v => _config.Needs.Hunger.Enabled = v);

        gmcmApi.AddBoolOption(
            mod: manifest,
            name: () => "Thirst System",
            tooltip: () => "Enables the thirst hidden stat.",
            getValue: () => _config.Needs.Thirst.Enabled,
            setValue: v => _config.Needs.Thirst.Enabled = v);
    }

    private void Validate()
    {
        if (_config.Diapers.BaseAccidentChance is < 0f or > 1f)
        {
            _log.Warn("BaseAccidentChance {0} out of range [0,1]. Clamping.",
                _config.Diapers.BaseAccidentChance);
            _config.Diapers.BaseAccidentChance =
                Math.Clamp(_config.Diapers.BaseAccidentChance, 0f, 1f);
        }

        if (_config.Diapers.WetnessTickIntervalMinutes < 10)
        {
            _log.Warn("WetnessTickIntervalMinutes {0} too low (min 10). Correcting.",
                _config.Diapers.WetnessTickIntervalMinutes);
            _config.Diapers.WetnessTickIntervalMinutes = 10;
        }

        if (_config.Npcs.MinFriendshipForReaction is < 0 or > 14)
        {
            _log.Warn("MinFriendshipForReaction {0} out of range [0,14]. Clamping.",
                _config.Npcs.MinFriendshipForReaction);
            _config.Npcs.MinFriendshipForReaction =
                Math.Clamp(_config.Npcs.MinFriendshipForReaction, 0, 14);
        }

        if (_config.Comfort.MaxComfort <= 0f)
        {
            _log.Warn("MaxComfort must be positive. Resetting to 100.");
            _config.Comfort.MaxComfort = 100f;
        }

        if (_config.Needs.Continence.TickIntervalMinutes < 10)
        {
            _log.Warn("Continence TickIntervalMinutes {0} too low (min 10). Correcting.",
                _config.Needs.Continence.TickIntervalMinutes);
            _config.Needs.Continence.TickIntervalMinutes = 10;
        }
    }
}

// ---------------------------------------------------------------------------
// Minimal duck-typed GMCM API interface.
// ---------------------------------------------------------------------------

/// <summary>
/// Minimal interface for Generic Mod Config Menu's API.
/// Only the methods used by this mod are declared.
/// GMCM uses duck typing so we only need to match the method signatures.
/// </summary>
public interface IGmcmApi
{
    void Register(StardewModdingAPI.IManifest mod, Action reset, Action save,
        bool titleScreenOnly = false);

    void AddSectionTitle(StardewModdingAPI.IManifest mod, Func<string> text,
        Func<string>? tooltip = null);

    void AddBoolOption(StardewModdingAPI.IManifest mod, Func<bool> getValue, Action<bool> setValue,
        Func<string> name, Func<string>? tooltip = null,
        string? fieldId = null);

    void AddNumberOption(StardewModdingAPI.IManifest mod, Func<int> getValue, Action<int> setValue,
        Func<string> name, Func<string>? tooltip = null,
        int? min = null, int? max = null, int? interval = null,
        string? fieldId = null);

    void AddNumberOption(StardewModdingAPI.IManifest mod, Func<float> getValue, Action<float> setValue,
        Func<string> name, Func<string>? tooltip = null,
        float? min = null, float? max = null, float? interval = null,
        string? fieldId = null);
}
