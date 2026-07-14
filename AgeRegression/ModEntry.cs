using AgeRegression.Api;
using AgeRegression.Commands;
using AgeRegression.Config;
using AgeRegression.Data;
using AgeRegression.Dialogue;
using AgeRegression.Events;
using AgeRegression.Integrations;
using AgeRegression.Items;
using AgeRegression.Patches;
using AgeRegression.Persistence;
using AgeRegression.Shops;
using AgeRegression.State;
using AgeRegression.Systems;
using AgeRegression.UI;
using AgeRegression.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace AgeRegression;

public sealed class ModEntry : Mod
{
    // -------------------------------------------------------------------------
    // Core services
    // -------------------------------------------------------------------------

    private LogHelper _log = null!;
    private ConfigManager _configManager = null!;
    private ModEventBus _eventBus = null!;
    private DataLoader _dataLoader = null!;
    private StateManager _stateManager = null!;
    private PersistenceManager _persistence = null!;
    private PatchManager _patchManager = null!;

    // -------------------------------------------------------------------------
    // Systems
    // -------------------------------------------------------------------------

    private RegressionSystem _regressionSystem = null!;
    private DiaperSystem _diaperSystem = null!;
    private ComfortSystem _comfortSystem = null!;
    private MoodSystem _moodSystem = null!;
    private NeedsSystem _needsSystem = null!;
    private WardrobeSystem _wardrobeSystem = null!;
    private CareSystem _careSystem = null!;
    private NpcReactionSystem _npcReactionSystem = null!;
    private FurnitureProximitySystem _furnitureProximitySystem = null!;
    private NotificationSystem _notificationSystem = null!;
    private StatusHud _statusHud = null!;
    private GameEventManager _gameEventManager = null!;

    // -------------------------------------------------------------------------
    // Item handlers
    // -------------------------------------------------------------------------

    private ItemRegistry _itemRegistry = null!;
    private ItemFactory _itemFactory = null!;
    private WardrobeItemResolver _wardrobeItemResolver = null!;
    private ShopStockProvider _shopStockProvider = null!;
    private WardrobeShopIntegration _wardrobeShopIntegration = null!;
    private DiaperInteractionHandler _diaperInteractionHandler = null!;
    private AccessoryInteractionHandler _accessoryInteractionHandler = null!;
    private ConsoleCommands _consoleCommands = null!;

    // -------------------------------------------------------------------------
    // Entry point
    // -------------------------------------------------------------------------

    public override void Entry(IModHelper helper)
    {
        _log = new LogHelper(Monitor);

        var config = helper.ReadConfig<ModConfig>();
        _configManager = new ConfigManager(config, helper, _log);

        _eventBus   = new ModEventBus(_log);
        _persistence = new PersistenceManager(helper, _log);

        var assetProvider = new FileSystemAssetProvider(helper.DirectoryPath, _log);
        _dataLoader = new DataLoader(assetProvider, _log);
        _dataLoader.LoadAll();

        _stateManager = new StateManager(_dataLoader, _persistence, _eventBus, _log);

        _itemRegistry = new ItemRegistry(_dataLoader, helper, _log);
        _itemFactory = new ItemFactory(_dataLoader, _log);
        _wardrobeItemResolver = new WardrobeItemResolver(_dataLoader);
        var gameStateProvider = new StardewGameStateProvider();
        var unlockService     = new ItemUnlockService(gameStateProvider);
        var acquisitionService = new ItemAcquisitionService(
            _wardrobeItemResolver,
            unlockService,
            _itemFactory);
        _shopStockProvider = new ShopStockProvider(_dataLoader, unlockService);
        _wardrobeShopIntegration = new WardrobeShopIntegration(
            _shopStockProvider, helper, _log);

        BuildSystems(helper, unlockService, acquisitionService);
        RegisterProviders();
        RegisterSmApiEvents(helper);

        _itemRegistry.Register();
        _wardrobeShopIntegration.Register();
        _gameEventManager.Register();
        _consoleCommands.Register();
        _patchManager.ApplyAll(
            _regressionSystem,
            _npcReactionSystem);

        _configManager.RegisterWithGmcm();

        _log.Info("Age Regression mod loaded (v{0}).", ModManifest.Version);
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public override object? GetApi() =>
        new AgeRegressionApi(_stateManager, _dataLoader, _eventBus, _log);

    // -------------------------------------------------------------------------
    // Item access (for future console commands / shop integration)
    // -------------------------------------------------------------------------

    public ItemFactory ItemFactory => _itemFactory;

    // -------------------------------------------------------------------------
    // System construction
    // -------------------------------------------------------------------------

    private void BuildSystems(
        IModHelper helper,
        ItemUnlockService unlockService,
        ItemAcquisitionService acquisitionService)
    {
        var cfg = _configManager.Config;

        _regressionSystem = new RegressionSystem(
            _stateManager, _dataLoader, cfg, _eventBus, _log);

        _diaperSystem = new DiaperSystem(
            _stateManager, _dataLoader, cfg, _eventBus, _log);

        var conditionEvaluator  = new DialogueConditionEvaluator();
        var eventConditionEval  = new EventConditionEvaluator(conditionEvaluator, _dataLoader, cfg);

        _comfortSystem = new ComfortSystem(
            _stateManager, _dataLoader, conditionEvaluator, cfg, _eventBus, _log);

        _moodSystem = new MoodSystem(
            _stateManager, _dataLoader, cfg, _eventBus, _log);

        _needsSystem = new NeedsSystem(
            _stateManager, _dataLoader, cfg, _eventBus, _log);

        _wardrobeSystem = new WardrobeSystem(
            _stateManager, _dataLoader, cfg, _log);

        _careSystem = new CareSystem(
            _stateManager, _eventBus, _log);

        _furnitureProximitySystem = new FurnitureProximitySystem(
            _stateManager, _dataLoader, cfg, _eventBus, _log);

        _notificationSystem = new NotificationSystem(
            _stateManager, _dataLoader, cfg, _eventBus, _log);

        _statusHud = new StatusHud(
            _stateManager, _dataLoader, cfg, _eventBus, _log);

        var router        = new NpcReactionRouter(_dataLoader, _log);
        var tokenResolver = new DialogueTokenResolver(_dataLoader);
        var dialogueManager = new DialogueManager(
            router, conditionEvaluator, tokenResolver,
            _stateManager, _dataLoader, cfg, _log);

        _npcReactionSystem = new NpcReactionSystem(
            dialogueManager, _stateManager, cfg, _eventBus, _log);

        _diaperInteractionHandler = new DiaperInteractionHandler(
            _diaperSystem, _stateManager, cfg, _log);

        _accessoryInteractionHandler = new AccessoryInteractionHandler(
            _wardrobeSystem, _comfortSystem, _dataLoader, _stateManager, cfg, _log);

        _gameEventManager = new GameEventManager(
            _dataLoader, _stateManager, eventConditionEval,
            new FileSystemAssetProvider(helper.DirectoryPath, _log),
            helper, _log);

        _patchManager = new PatchManager(helper, _log, _dataLoader);

        _consoleCommands = new ConsoleCommands(
            helper,
            _stateManager,
            _diaperSystem,
            _comfortSystem,
            _dataLoader,
            acquisitionService,
            _configManager.Config,
            _log);
    }

    private void RegisterProviders()
    {
        _regressionSystem.RegisterModifierProvider(
            new MoodStatModifierProvider(_stateManager, _dataLoader, _log));
        _regressionSystem.RegisterModifierProvider(
            new FurnitureStatModifierProvider(_furnitureProximitySystem, _dataLoader, _log));
        _regressionSystem.RegisterModifierProvider(
            new ItemStatModifierProvider(_stateManager, _dataLoader, _log));
    }

    // -------------------------------------------------------------------------
    // SMAPI event hooks
    // -------------------------------------------------------------------------

    private void RegisterSmApiEvents(IModHelper helper)
    {
        helper.Events.GameLoop.SaveLoaded   += OnSaveLoaded;
        helper.Events.GameLoop.DayStarted   += OnDayStarted;
        helper.Events.GameLoop.TimeChanged  += OnTimeChanged;
        helper.Events.GameLoop.Saving       += OnSaving;
        helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
        helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        helper.Events.Display.Rendered      += OnRendered;
        helper.Events.Input.ButtonPressed   += OnButtonPressed;
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        _stateManager.LoadForCurrentPlayer();
        _comfortSystem.OnSaveLoaded();
        _needsSystem.OnSaveLoaded();
        _furnitureProximitySystem.Reset();
        _notificationSystem.OnSaveLoaded();
        _statusHud.OnSaveLoaded();
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        _stateManager.OnDayStarted();
        _careSystem.OnDayStarted();
        _diaperSystem.OnDayStarted();
        _comfortSystem.OnDayStarted();
        _needsSystem.OnDayStarted();
        _gameEventManager.OnDayStarted();
    }

    private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
    {
        _stateManager.OnTimeChanged(e.NewTime);
        _diaperSystem.OnTimeChanged(e.NewTime);
        _comfortSystem.OnTimeChanged(e.NewTime);
        _needsSystem.OnTimeChanged(e.NewTime);
    }

    private void OnSaving(object? sender, SavingEventArgs e)
    {
        _stateManager.SaveForCurrentPlayer();
    }

    private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        _stateManager.Unload();
        _furnitureProximitySystem.Reset();
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        _furnitureProximitySystem.OnUpdateTicked();
    }

    private void OnRendered(object? sender, RenderedEventArgs e)
    {
        if (_statusHud.ShouldRender())
        {
            HudRenderer.Render(_statusHud, e.SpriteBatch);
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        _log.Debug("ButtonPressed detected: {0}", e.Button);

        if (!Context.IsWorldReady)
        {
            _log.Debug("ButtonPressed rejected: world not ready");
            return;
        }

        // Block if an unrelated menu is open (shop, dialogue, etc.).
        // Do NOT block for GameMenu (inventory) — player can use items from the hotbar.
        var menu = StardewValley.Game1.activeClickableMenu;
        if (menu != null && menu is not StardewValley.Menus.GameMenu)
        {
            _log.Debug("ButtonPressed rejected: non-inventory menu open ({0})", menu.GetType().Name);
            return;
        }

        if (!e.Button.IsActionButton())
        {
            _log.Debug("ButtonPressed rejected: not an action button");
            return;
        }

        var player = StardewValley.Game1.player;
        if (player is null)
        {
            _log.Debug("ButtonPressed rejected: player is null");
            return;
        }

        var currentItem = player.CurrentItem;
        _log.Debug("Active item: {0}", currentItem?.QualifiedItemId ?? "(null)");

        if (currentItem is not StardewValley.Object obj)
        {
            _log.Debug("ButtonPressed rejected: active item is not Object type");
            return;
        }

        _log.Debug("Item use detected: {0}", obj.QualifiedItemId);
        _log.Debug("Item modData keys: {0}",
            obj.modData.Keys.Any() ? string.Join(", ", obj.modData.Keys) : "(empty)");

        // modData may be absent on items spawned externally (e.g. CJB Item Spawner).
        // Fall back to parsing the type ID from the qualified item ID, then seed
        // modData on the live instance so it behaves identically to factory-created items.

        // --- Diaper items ---
        if (!obj.modData.TryGetValue(ItemIds.ModDataDiaperTypeId, out var diaperTypeId))
            diaperTypeId = ItemIds.TryParseDiaperTypeId(obj.QualifiedItemId);

        if (diaperTypeId is not null)
        {
            if (!obj.modData.ContainsKey(ItemIds.ModDataDiaperTypeId))
            {
                _log.Debug("Seeding missing modData for diaper item: {0}", diaperTypeId);
                obj.modData[ItemIds.ModDataDiaperTypeId]   = diaperTypeId;
                obj.modData[ItemIds.ModDataWetnessLevel]   = "0";
                obj.modData[ItemIds.ModDataMessingLevel]   = "0";
                obj.modData[ItemIds.ModDataHasBooster]     = "False";
                obj.modData[ItemIds.ModDataLastChangedDay] =
                    StardewValley.Game1.stats.DaysPlayed.ToString();
            }

            _log.Debug("Diaper detected: {0}", diaperTypeId);
            var hasBooster =
                obj.modData.TryGetValue(ItemIds.ModDataHasBooster, out var boosterRaw)
                && bool.TryParse(boosterRaw, out var booster)
                && booster;

            if (_diaperInteractionHandler.HandleUseItem(diaperTypeId, hasBooster))
            {
                _log.Debug("Diaper action completed: {0}", diaperTypeId);
                player.reduceActiveItemByOne();
                Helper.Input.Suppress(e.Button);
                return;
            }
        }

        // --- Accessory items ---
        if (!obj.modData.TryGetValue(ItemIds.ModDataAccessoryTypeId, out var accessoryId))
            accessoryId = ItemIds.TryParseAccessoryTypeId(obj.QualifiedItemId);

        if (accessoryId is not null)
        {
            if (!obj.modData.ContainsKey(ItemIds.ModDataAccessoryTypeId))
            {
                _log.Debug("Seeding missing modData for accessory item: {0}", accessoryId);
                obj.modData[ItemIds.ModDataAccessoryTypeId] = accessoryId;
            }

            _log.Debug("Accessory detected: {0}", accessoryId);
            if (_accessoryInteractionHandler.HandleUseItem(accessoryId))
            {
                _log.Debug("Accessory action completed: {0}", accessoryId);
                player.reduceActiveItemByOne();
                Helper.Input.Suppress(e.Button);
            }
        }
    }
}