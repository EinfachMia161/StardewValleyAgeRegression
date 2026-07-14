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
    private readonly ItemAcquisitionService _acquisitionService;
    private readonly ModConfig _config;
    private readonly LogHelper _log;

    public ConsoleCommands(
        IModHelper helper,
        StateManager stateManager,
        DiaperSystem diaperSystem,
        ComfortSystem comfortSystem,
        DataLoader dataLoader,
        ItemAcquisitionService acquisitionService,
        ModConfig config,
        LogHelper log)
    {
        _helper        = helper;
        _stateManager  = stateManager;
        _diaperSystem  = diaperSystem;
        _comfortSystem = comfortSystem;
        _dataLoader          = dataLoader;
        _acquisitionService  = acquisitionService;
        _config              = config;
        _log                 = log;
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
            "age_regression give",
            "Gives the player any wardrobe item (diaper or accessory).\n" +
            "Usage: age_regression give <itemId> [booster:true|false]\n" +
            "Diaper IDs: " + ValidDiaperIds() + "\n" +
            "Accessory IDs: " + ValidAccessoryIds(),
            CmdGive);

        // Deprecated: use 'age_regression give <itemId>' instead.
        _helper.ConsoleCommands.Add(
            "age_regression.give_diaper",
            "[Deprecated] Use 'age_regression give <itemId>'.\n" +
            "Gives the player a diaper item. Optionally include a booster.\n" +
            "Usage: age_regression.give_diaper <diaperId> [true|false]\n" +
            "Valid IDs: " + ValidDiaperIds(),
            CmdGiveDiaper);

        // Deprecated: use 'age_regression give <itemId>' instead.
        _helper.ConsoleCommands.Add(
            "age_regression.give_accessory",
            "[Deprecated] Use 'age_regression give <itemId>'.\n" +
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

    private void CmdGive(string cmd, string[] args)
    {
        if (!AssertWorldReady()) return;

        if (args.Length < 1)
        {
            Error("Usage: age_regression give <itemId> [booster:true|false]");
            Info("Diaper IDs: " + ValidDiaperIds());
            Info("Accessory IDs: " + ValidAccessoryIds());
            return;
        }

        var itemId     = args[0].Trim().ToLowerInvariant();
        var hasBooster = args.Length >= 2 &&
                         bool.TryParse(args[1], out var b) && b;

        var result = _acquisitionService.Acquire(
            itemId,
            new AcquisitionContext(
                Source: AcquisitionSource.Console,
                HasBooster: hasBooster,
                CurrentAbsoluteDay: AbsoluteDayHelper.GetCurrentAbsoluteDay()));

        if (!result.Success)
        {
            switch (result.FailureReason)
            {
                case AcquisitionFailureReason.UnknownItem:
                    Error($"Unknown item ID '{itemId}'.");
                    Info("Diaper IDs: " + ValidDiaperIds());
                    Info("Accessory IDs: " + ValidAccessoryIds());
                    break;
                case AcquisitionFailureReason.Locked:
                    Error($"Item '{itemId}' is currently locked.");
                    break;
                case AcquisitionFailureReason.CreationFailed:
                    Error($"ItemFactory returned null for '{itemId}'. Check SMAPI log.");
                    break;
                default:
                    Error($"Acquisition failed for '{itemId}'.");
                    break;
            }
            return;
        }

        if (result.CreatedItem is null || result.ResolvedItem is null)
        {
            Error($"Acquisition returned no item for '{itemId}'.");
            return;
        }

        Game1.player.addItemByMenuIfNecessary(result.CreatedItem);
        var suffix = result.ResolvedItem.Category == WardrobeCategory.Diaper && hasBooster
            ? " (with booster)." : ".";
        Ok($"Gave {result.ResolvedItem.Category.ToString().ToLowerInvariant()} '{itemId}'{suffix}");
    }

    // Deprecated: use CmdGive via 'age_regression give <itemId>' instead.
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

        var result = _acquisitionService.Acquire(
            diaperId,
            new AcquisitionContext(
                Source: AcquisitionSource.Console,
                HasBooster: hasBooster,
                CurrentAbsoluteDay: AbsoluteDayHelper.GetCurrentAbsoluteDay()));

        if (!result.Success)
        {
            switch (result.FailureReason)
            {
                case AcquisitionFailureReason.UnknownItem:
                    Error($"Unknown diaper type '{diaperId}'.");
                    Info("Valid IDs: " + ValidDiaperIds());
                    break;
                case AcquisitionFailureReason.Locked:
                    Error($"Diaper '{diaperId}' is currently locked.");
                    break;
                case AcquisitionFailureReason.CreationFailed:
                    Error($"ItemFactory returned null for '{diaperId}'. Check SMAPI log.");
                    break;
                default:
                    Error($"Acquisition failed for '{diaperId}'.");
                    break;
            }
            return;
        }

        if (result.CreatedItem is null)
        {
            Error($"Acquisition returned no item for '{diaperId}'.");
            return;
        }

        Game1.player.addItemByMenuIfNecessary(result.CreatedItem);
        Ok($"Gave diaper '{diaperId}'" + (hasBooster ? " (with booster)." : "."));
    }

    // Deprecated: use CmdGive via 'age_regression give <itemId>' instead.
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

        var result = _acquisitionService.Acquire(
            accessoryId,
            new AcquisitionContext(Source: AcquisitionSource.Console));

        if (!result.Success)
        {
            switch (result.FailureReason)
            {
                case AcquisitionFailureReason.UnknownItem:
                    Error($"Unknown accessory '{accessoryId}'.");
                    Info("Valid IDs: " + ValidAccessoryIds());
                    break;
                case AcquisitionFailureReason.Locked:
                    Error($"Accessory '{accessoryId}' is currently locked.");
                    break;
                case AcquisitionFailureReason.CreationFailed:
                    Error($"ItemFactory returned null for '{accessoryId}'. Check SMAPI log.");
                    break;
                default:
                    Error($"Acquisition failed for '{accessoryId}'.");
                    break;
            }
            return;
        }

        if (result.CreatedItem is null)
        {
            Error($"Acquisition returned no item for '{accessoryId}'.");
            return;
        }

        Game1.player.addItemByMenuIfNecessary(result.CreatedItem);
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
