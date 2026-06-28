using AgeRegression.Config;
using AgeRegression.Data;
using AgeRegression.Events;
using AgeRegression.State;
using AgeRegression.Utilities;
using DataLoader = AgeRegression.Data.DataLoader;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgeRegression.Systems;

/// <summary>
/// Detects when the player is near comfort-providing furniture and
/// publishes <see cref="FurnitureProximityChangedEventArgs"/> when
/// proximity state changes.
///
/// <para>
/// Proximity is checked on SMAPI's <c>UpdateTicked</c> event, throttled
/// to once per second (60 ticks) to avoid performance impact.
/// No Harmony patches are used.
/// </para>
///
/// <para>
/// <see cref="ComfortSystem"/> subscribes to
/// <see cref="FurnitureProximityChangedEventArgs"/> directly and
/// recalculates active comfort modifiers in response.
/// </para>
/// </summary>
public sealed class FurnitureProximitySystem
{
    private readonly StateManager _stateManager;
    private readonly DataLoader _dataLoader;
    private readonly ModConfig _config;
    private readonly ModEventBus _eventBus;
    private readonly LogHelper _log;

    private readonly HashSet<string> _activeModifierIds = new();

    private int _tickCounter = 0;
    private const int CheckIntervalTicks = 60;

    public FurnitureProximitySystem(
        StateManager stateManager,
        DataLoader dataLoader,
        ModConfig config,
        ModEventBus eventBus,
        LogHelper log)
    {
        _stateManager = stateManager;
        _dataLoader   = dataLoader;
        _config       = config;
        _eventBus     = eventBus;
        _log          = log;
    }

    public IReadOnlySet<string> ActiveModifierIds => _activeModifierIds;

    public void OnUpdateTicked()
    {
        if (!_config.Enabled) return;

        _tickCounter++;
        if (_tickCounter < CheckIntervalTicks) return;
        _tickCounter = 0;

        CheckProximity();
    }

    public void Reset()
    {
        _activeModifierIds.Clear();
        _tickCounter = 0;
    }

    private void CheckProximity()
    {
        if (!GameHelper.IsWorldReady()) return;

        var state = _stateManager.GetCurrentState();
        if (state is null) return;

        var farmer   = Game1.player;
        var location = farmer.currentLocation;
        if (location is null) return;

        var newActiveIds = new HashSet<string>();

        foreach (var comfortDef in _dataLoader.FurnitureComfort)
        {
            if (IsNearFurniture(
                    farmer, location,
                    comfortDef.FurnitureId,
                    comfortDef.ProximityTiles))
            {
                newActiveIds.Add(comfortDef.ComfortModifierId);
            }
        }

        if (newActiveIds.SetEquals(_activeModifierIds))
            return;

        var added   = newActiveIds.Except(_activeModifierIds).ToList();
        var removed = _activeModifierIds.Except(newActiveIds).ToList();

        _activeModifierIds.Clear();
        foreach (var id in newActiveIds)
            _activeModifierIds.Add(id);

        _log.Debug(
            "Furniture proximity changed. Added: [{0}] Removed: [{1}].",
            string.Join(", ", added),
            string.Join(", ", removed));

        _eventBus.Publish(new FurnitureProximityChangedEventArgs(
            added.AsReadOnly(),
            removed.AsReadOnly(),
            _activeModifierIds,
            state.PlayerId));
    }

    private static bool IsNearFurniture(
        Farmer farmer,
        GameLocation location,
        string furnitureId,
        int proximityTiles)
    {
        var qualifiedId = $"(F)mia.AgeRegression_{furnitureId}";
        var farmerTile  = farmer.TilePoint;

        foreach (var furniture in location.furniture)
        {
            if (!furniture.QualifiedItemId.Equals(
                    qualifiedId, StringComparison.OrdinalIgnoreCase))
                continue;

            var furnitureTile = furniture.TileLocation;
            var distance =
                Math.Abs(farmerTile.X - (int)furnitureTile.X)
                + Math.Abs(farmerTile.Y - (int)furnitureTile.Y);

            if (distance <= proximityTiles)
                return true;
        }

        return false;
    }
}
