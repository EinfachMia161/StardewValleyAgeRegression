using System;
using AgeRegression.Data;
using AgeRegression.State;
using AgeRegression.Systems;
using AgeRegression.Utilities;

namespace AgeRegression.Items;

/// <summary>
/// <see cref="IStatModifierProvider" /> that contributes stat modifiers
/// from currently equipped items (diapers and accessories).
///
/// <para>
/// Currently handles:
/// <list type="bullet">
///   <item>Diaper thickness speed penalty
///   (<see cref="DiaperTypeData.ThicknessSpeedPenalty" />)</item>
///   <item>Accessory speed modifiers
///   (<see cref="WardrobeItemData.SpeedModifier" />)</item>
/// </list>
/// All values come from data files. No item effects are hardcoded here.
/// </para>
/// </summary>
public sealed class ItemStatModifierProvider : IStatModifierProvider
{
    private readonly StateManager _stateManager;
    private readonly DataLoader _dataLoader;
    private readonly LogHelper _log;

    /// <inheritdoc />
    public string ProviderId => "equipped_items";

    public ItemStatModifierProvider(
        StateManager stateManager,
        DataLoader dataLoader,
        LogHelper log)
    {
        _stateManager = stateManager;
        _dataLoader   = dataLoader;
        _log          = log;
    }

    /// <inheritdoc />
    public StatModifierContribution GetContribution()
    {
        var state = _stateManager.GetCurrentState();
        if (state is null)
            return StatModifierContribution.Identity;

        var speed = 1.0f;

        // Diaper thickness speed penalty
        if (state.Diaper.IsWearingDiaper)
        {
            var diaperType = _dataLoader.GetDiaperType(
                state.Diaper.EquippedDiaperTypeId!);

            if (diaperType is not null)
                speed *= diaperType.ThicknessSpeedPenalty;
        }

        // Accessory speed modifiers
        foreach (var accessoryId in state.EquippedAccessories)
        {
            var accessory = _dataLoader.GetWardrobeItem(accessoryId);
            if (accessory is not null)
                speed *= accessory.SpeedModifier;
        }

        if (Math.Abs(speed - 1f) < 0.001f)
            return StatModifierContribution.Identity;

        return new StatModifierContribution(speed, 1f, 1f, true);
    }
}
