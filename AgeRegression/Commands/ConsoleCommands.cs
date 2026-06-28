using AgeRegression.Config;
using AgeRegression.Data;
using AgeRegression.Items;
using AgeRegression.Persistence;
using AgeRegression.State;
using AgeRegression.Systems;
using AgeRegression.Utilities;
using StardewModdingAPI;
using StardewValley;
using DataLoader = AgeRegression.Data.DataLoader;

namespace AgeRegression.Commands;

/// <summary>
/// Registers all <c>age_regression.*</c> SMAPI console commands.
/// All state changes go through the existing public APIs — no direct
/// save or state mutation happens here.
/// </summary>
public sealed class ConsoleCommands
{
    private readonly IModHelper _helper;
    private readonly StateManager _stateManager;
    private readonly DiaperSystem _diaperSystem;
    private readonly ComfortSystem _comfortSystem;
    private readonly DataLoader _dataLoader;
    private readonly ItemFactory _itemFactory;
    private readonly ModConfig _config;
    private readonly LogHelper _log;

    public ConsoleCommands(
        IModHelper helper,
        StateManager stateManager,
        DiaperSystem diaperSystem,
        ComfortSystem comfortSystem,
        DataLoader dataLoader,
        ItemFactory itemFactory,
        ModConfig config,
        LogHelper log)
    {
        _helper        = helper;
        _stateManager  = stateManager;
        _diaperSystem  = diaperSystem;
        _comfortSystem = comfortSystem;
        _dataLoader    = dataLoader;
        _itemFactory   = itemFactory;
        _config        = config;
        _log           = log;
    }

    /// <summary>Registers all commands with SMAPI. Call once during Entry.</summary>
    public void Register()
    {
        _helper.ConsoleCommands.Add(
            "age_regression.stage",
            "Sets the player's regression stage.\n" +
            "Usage: age_regression.stage <stageId>\n" +
            "Valid IDs: " + ValidStageIds(),
            CmdStage);

        _helper.ConsoleCommands.Add(
            "age_regression.give_diaper",
            "Gives the player a diaper item. Optionally include a booster.\n" +
            "Usage: age_regression.give_diaper <diaperId> [true|false]\n" +
            "Valid IDs: " + ValidDiaperIds(),
            CmdGiveDiaper);

        _helper.ConsoleCommands.Add(
            "age_regression.give_accessory",
            "Gives the player an accessory item.\n" +
            "Usage: age_regression.give_accessory <accessoryId>\n" +
            "Valid IDs: " + ValidAccessoryIds(),
            CmdGiveAccessory);

        _helper.ConsoleCommands.Add(
            "age_regression.comfort",
            "Sets the player's comfort score to an exact value.\n" +
            "Usage: age_regression.comfort <value>  (0 to " +
                _config.Comfort.MaxComfort + ")",
            CmdComfort);

        _helper.ConsoleCommands.Add(
            "age_regression.reset",
            "Resets regression state to defaults and saves immediately.\n" +
            "Usage: age_regression.reset",
            CmdReset);

        _helper.ConsoleCommands.Add(
            "age_regression.debug",
            "Prints the current regression state to the SMAPI console.\n" +
            "Usage: age_regression.debug",
            CmdDebug);

        _log.Debug("Console commands registered.");
    }

    // -------------------------------------------------------------------------
    // Command handlers
    // -------------------------------------------------------------------------

    private void CmdStage(string cmd, string[] args)
    {
        if (!AssertWorldReady()) return;

        if (args.Length != 1)
        {
            Error("Usage: age_regression.stage <stageId>");
            Info("Valid IDs: " + ValidStageIds());
            return;
        }

        var stageId = args[0].Trim().ToLowerInvariant();
        var stage   = _dataLoader.GetStage(stageId);
        if (stage is null)
        {
            Error($"Unknown stage ID '{stageId}'.");
            Info("Valid IDs: " + ValidStageIds());
            return;
        }

        if (_stateManager.TrySetStage(stageId))
            Ok($"Stage set to '{stage.DisplayName}' ({stageId}).");
        else
            Error($"Stage transition failed for '{stageId}'. Check SMAPI log.");
    }

    private void CmdGiveDiaper(string cmd, string[] args)
    {
        if (!AssertWorldReady()) return;

        if (args.Length < 1)
        {
            Error("Usage: age_regression.give_diaper <diaperId> [true|false]");
            Info("Valid IDs: " + ValidDiaperIds());
            return;
        }

        var diaperId   = args[0].Trim().ToLowerInvariant();
        var hasBooster = args.Length >= 2 &&
                         bool.TryParse(args[1], out var b) && b;

        if (_dataLoader.GetDiaperType(diaperId) is null)
        {
            Error($"Unknown diaper type '{diaperId}'.");
            Info("Valid IDs: " + ValidDiaperIds());
            return;
        }

        var item = _itemFactory.CreateDiaper(
            diaperId, hasBooster, AbsoluteDayHelper.GetCurrentAbsoluteDay());

        if (item is null)
        {
            Error($"ItemFactory returned null for '{diaperId}'. Check SMAPI log.");
            return;
        }

        Game1.player.addItemByMenuIfNecessary(item);
        Ok($"Gave diaper '{diaperId}'" + (hasBooster ? " (with booster)." : "."));
    }

    private void CmdGiveAccessory(string cmd, string[] args)
    {
        if (!AssertWorldReady()) return;

        if (args.Length != 1)
        {
            Error("Usage: age_regression.give_accessory <accessoryId>");
            Info("Valid IDs: " + ValidAccessoryIds());
            return;
        }

        var accessoryId = args[0].Trim().ToLowerInvariant();

        if (_dataLoader.GetWardrobeItem(accessoryId) is null)
        {
            Error($"Unknown accessory '{accessoryId}'.");
            Info("Valid IDs: " + ValidAccessoryIds());
            return;
        }

        var item = _itemFactory.CreateAccessory(accessoryId);

        if (item is null)
        {
            Error($"ItemFactory returned null for '{accessoryId}'. Check SMAPI log.");
            return;
        }

        Game1.player.addItemByMenuIfNecessary(item);
        Ok($"Gave accessory '{accessoryId}'.");
    }

    private void CmdComfort(string cmd, string[] args)
    {
        if (!AssertWorldReady()) return;

        if (args.Length != 1 ||
            !float.TryParse(
                args[0],
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var target))
        {
            Error($"Usage: age_regression.comfort <value>  (0 to {_config.Comfort.MaxComfort})");
            return;
        }

        target = Math.Clamp(target, 0f, _config.Comfort.MaxComfort);

        var current = _stateManager.GetCurrentState()?.Comfort.CurrentComfort ?? 0f;
        var delta   = target - current;

        _comfortSystem.ApplyDirectAdjustment(delta, "console_command");
        Ok($"Comfort set to {target:F1}.");
    }

    private void CmdReset(string cmd, string[] args)
    {
        if (!AssertWorldReady()) return;

        var farmer   = Game1.player;
        var newState = PersistenceManager.StateFromSaveData(
            new SaveDataModel(), farmer.UniqueMultiplayerID);

        _stateManager.ForceLoadState(newState);
        _stateManager.SaveForCurrentPlayer();

        Ok("Regression state reset to defaults and saved.");
    }

    private void CmdDebug(string cmd, string[] args)
    {
        if (!AssertWorldReady()) return;

        var state = _stateManager.GetCurrentState();
        if (state is null)
        {
            Error("No state loaded.");
            return;
        }

        var stage = _dataLoader.GetStage(state.CurrentStageId);
        var sb    = new System.Text.StringBuilder();

        sb.AppendLine("=== Age Regression Debug ===");
        sb.AppendLine($"  Stage      : {state.CurrentStageId}" +
            (stage is not null
                ? $" ({stage.DisplayName}, order {stage.Order})"
                : " (unknown)"));
        sb.AppendLine($"  Comfort    : {state.Comfort.CurrentComfort:F1}" +
            $" / {_config.Comfort.MaxComfort:F0}" +
            $"  [{state.Comfort.GetNormalized(_config.Comfort.MaxComfort):P0}]");
        sb.AppendLine($"  Mood       : " +
            (string.IsNullOrEmpty(state.Mood.CurrentMoodId)
                ? "(none)"
                : state.Mood.CurrentMoodId));

        if (state.Diaper.IsWearingDiaper)
            sb.AppendLine($"  Diaper     : {state.Diaper.EquippedDiaperTypeId}" +
                $"  condition={state.Diaper.ConditionId}" +
                $"  wet={state.Diaper.WetnessLevel:P0}" +
                $"  mess={state.Diaper.MessingLevel:P0}" +
                $"  booster={state.Diaper.HasBooster}");
        else
            sb.AppendLine("  Diaper     : none");

        sb.AppendLine($"  Accessories: " +
            (state.EquippedAccessories.Count > 0
                ? string.Join(", ", state.EquippedAccessories)
                : "none"));
        sb.AppendLine($"  Continence : {state.Needs.Continence.Value.Normalized:P0}" +
            $"  [{state.Needs.Continence.Value.LastKnownThresholdId}]" +
            $"  stress={state.Needs.Continence.StressModifier:+0.00;-0.00}");
        sb.AppendLine($"  Hunger     : {state.Needs.Hunger.Normalized:P0}" +
            $"  [{state.Needs.Hunger.LastKnownThresholdId}]");
        sb.AppendLine($"  Thirst     : {state.Needs.Thirst.Normalized:P0}" +
            $"  [{state.Needs.Thirst.LastKnownThresholdId}]");
        sb.AppendLine($"  Accidents  : {state.AccidentsToday} today");

        // Trim trailing newline before logging
        _log.Info(sb.ToString().TrimEnd());
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private bool AssertWorldReady()
    {
        if (Context.IsWorldReady) return true;
        Error("No save loaded. Load a save first.");
        return false;
    }

    private void Ok(string msg)    => _log.Info("[CMD] " + msg);
    private void Info(string msg)  => _log.Info("[CMD] " + msg);
    private void Error(string msg) => _log.Warn("[CMD] " + msg);

    private string ValidStageIds() =>
        string.Join(", ", _dataLoader.Stages.Select(s => s.Id));

    private string ValidDiaperIds() =>
        string.Join(", ", _dataLoader.DiaperTypes.Select(d => d.Id));

    private string ValidAccessoryIds() =>
        string.Join(", ", _dataLoader.WardrobeItems.Select(w => w.Id));
}
