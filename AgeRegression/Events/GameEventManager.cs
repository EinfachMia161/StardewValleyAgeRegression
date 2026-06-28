using AgeRegression.Data;
using AgeRegression.State;
using AgeRegression.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace AgeRegression.Events;
/// <summary>
/// Manages registration and triggering of mod-defined game events.
///
/// <para>
/// Events are defined in <c>assets/data/events.json</c> and their
/// scripts in <c>assets/events/*.txt</c>. This manager:
/// <list type="number">
///   <item>Injects event scripts into
///   <c>Data/Events/{Location}</c> via <c>AssetRequested</c>.</item>
///   <item>Checks event conditions each day start.</item>
///   <item>Tracks which one-time events have been seen using
///   <c>farmer.modData</c> with absolute day numbers — the only
///   source of truth for seen-state.</item>
/// </list>
/// </para>
///
/// <para>
/// <b>Script loading:</b> Event scripts are plain text files and
/// cannot be loaded via <c>IModContentHelper.Load&lt;string&gt;</c>
/// (which requires XNA content pipeline compilation). Scripts are
/// loaded via <see cref="IAssetProvider.LoadRaw" /> instead.
/// </para>
///
/// <para>
/// <b>Seen-state tracking:</b> Uses <c>farmer.modData</c> with
/// absolute day numbers exclusively. No hash-based or integer event
/// ID tracking is used anywhere in this system.
/// </para>
/// </summary>
public sealed class GameEventManager
{
    private readonly DataLoader _dataLoader;
    private readonly StateManager _stateManager;
    private readonly EventConditionEvaluator _conditionEvaluator;
    private readonly IAssetProvider _assetProvider;
    private readonly IModHelper _helper;
    private readonly LogHelper _log;

    /// <summary>
    /// modData key prefix for tracking seen events.
    /// Format: <c>mia.AgeRegression/EventSeen/{EventId}</c>.
    /// Value: absolute day number when last seen.
    /// </summary>
    private const string SeenKeyPrefix = "mia.AgeRegression/EventSeen/";

    public GameEventManager(
        DataLoader dataLoader,
        StateManager stateManager,
        EventConditionEvaluator conditionEvaluator,
        IAssetProvider assetProvider,
        IModHelper helper,
        LogHelper log)
    {
        _dataLoader = dataLoader;
        _stateManager = stateManager;
        _conditionEvaluator = conditionEvaluator;
        _assetProvider = assetProvider;
        _helper = helper;
        _log = log;
    }

    // -------------------------------------------------------------------------
    // Registration
    // -------------------------------------------------------------------------

    /// <summary>
    /// Subscribes to <c>AssetRequested</c> to inject event scripts.
    /// Call once during <c>GameLaunched</c>.
    /// </summary>
    public void Register()
    {
        _helper.Events.Content.AssetRequested += OnAssetRequested;
        _log.Debug("GameEventManager registered.");
    }

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    /// <summary>Called at the start of each in-game day.</summary>
    public void OnDayStarted()
    {
        CheckPendingEvents();
    }

    // -------------------------------------------------------------------------
    // Asset injection
    // -------------------------------------------------------------------------

    private void OnAssetRequested(
        object? sender,
        AssetRequestedEventArgs e)
    {
        foreach (var eventDef in _dataLoader.EventDefinitions)
        {
            var targetAsset = $"Data/Events/{eventDef.Location}";
            if (!e.NameWithoutLocale.IsEquivalentTo(targetAsset))
                continue;

            e.Edit(
                asset => InjectEvent(asset, eventDef),
                AssetEditPriority.Default);
        }
    }

    private void InjectEvent(
        IAssetData asset,
        EventDefinitionData eventDef)
    {
        // Build the absolute path for LoadRaw.
        // Plain text files cannot be loaded via ModContent.Load<string>
        // because that requires XNA content pipeline compilation.
        var absolutePath = System.IO.Path.Combine(
            _helper.DirectoryPath,
            eventDef.ScriptFile.Replace(
                '/', System.IO.Path.DirectorySeparatorChar));

        var script = _assetProvider.LoadRaw(absolutePath);

        if (script is null)
        {
            _log.Warn(
                "Event '{0}': script file '{1}' could not be loaded.",
                eventDef.EventId, eventDef.ScriptFile);
            return;
        }

        // Trim whitespace — SV event scripts must not have leading or
        // trailing newlines
        script = script.Trim();

        if (string.IsNullOrWhiteSpace(script))
        {
            _log.Warn("Event '{0}': script file '{1}' is empty.",
                eventDef.EventId, eventDef.ScriptFile);
            return;
        }

        var data = asset.AsDictionary<string, string>().Data;

        var key = string.IsNullOrWhiteSpace(eventDef.NativePreconditions)
            ? eventDef.EventId
            : $"{eventDef.EventId}/{eventDef.NativePreconditions}";

        if (data.ContainsKey(key))
        {
            _log.Warn(
                "Event key '{0}' already exists in " +
                "'Data/Events/{1}'. Skipping.",
                key, eventDef.Location);
            return;
        }

        data[key] = script;
        _log.Debug(
            "Injected event '{0}' into 'Data/Events/{1}'.",
            eventDef.EventId, eventDef.Location);
    }

    // -------------------------------------------------------------------------
    // Condition checking
    // -------------------------------------------------------------------------

    private void CheckPendingEvents()
    {
        var state = _stateManager.GetCurrentState();
        if (state is null) return;

        var farmer = StardewValley.Game1.player;
        var today  = AbsoluteDayHelper.GetCurrentAbsoluteDay();

        foreach (var eventDef in _dataLoader.EventDefinitions
                     .OrderByDescending(e => e.Priority))
        {
            if (eventDef.OneTimeOnly &&
                HasBeenSeen(farmer, eventDef.EventId))
            {
                _log.Trace("Event '{0}' already seen — skipping.",
                    eventDef.EventId);
                continue;
            }

            if (!eventDef.OneTimeOnly)
            {
                var lastSeen  = GetLastSeenAbsoluteDay(
                    farmer, eventDef.EventId);
                var daysSince = AbsoluteDayHelper.DaysBetween(
                    lastSeen, today);

                if (daysSince < eventDef.RepeatCooldownDays)
                {
                    _log.Trace(
                        "Event '{0}' on cooldown ({1}/{2} days). " +
                        "Skipping.",
                        eventDef.EventId, daysSince,
                        eventDef.RepeatCooldownDays);
                    continue;
                }
            }

            if (_conditionEvaluator.AreConditionsMet(eventDef, state))
            {
                _log.Debug(
                    "Event '{0}' conditions met — eligible to trigger " +
                    "at '{1}'.",
                    eventDef.EventId, eventDef.Location);
            }
        }
    }

    // -------------------------------------------------------------------------
    // Seen tracking — farmer.modData is the only source of truth
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns <c>true</c> if the given one-time event has been seen.
    /// Uses <c>farmer.modData</c> exclusively.
    /// </summary>
    public bool HasBeenSeen(
        StardewValley.Farmer farmer,
        string eventId) =>
        farmer.modData.ContainsKey(SeenKeyPrefix + eventId);

    /// <summary>
    /// Returns the absolute day number when the event was last seen,
    /// or 0 if it has never been seen.
    /// </summary>
    public int GetLastSeenAbsoluteDay(
        StardewValley.Farmer farmer,
        string eventId)
    {
        var key = SeenKeyPrefix + eventId;
        if (farmer.modData.TryGetValue(key, out var raw) &&
            int.TryParse(raw, out var day))
            return day;
        return 0;
    }

    /// <summary>
    /// Marks an event as seen on the current absolute day.
    /// For one-time events, this permanently prevents re-triggering.
    /// For repeatable events, this starts the cooldown timer.
    /// </summary>
    public void MarkEventSeen(string eventId)
    {
        var farmer = StardewValley.Game1.player;
        var today  = AbsoluteDayHelper.GetCurrentAbsoluteDay();
        farmer.modData[SeenKeyPrefix + eventId] = today.ToString();
        _log.Debug("Marked event '{0}' as seen on absolute day {1}.",
            eventId, today);
    }
}