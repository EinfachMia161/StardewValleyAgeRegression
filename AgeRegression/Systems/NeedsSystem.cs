using AgeRegression.Config;
using AgeRegression.Data;
using AgeRegression.Events;
using AgeRegression.State;
using AgeRegression.Systems.Needs;
using AgeRegression.Utilities;

namespace AgeRegression.Systems;

/// <summary>
/// Orchestrates all needs subsystems (continence, hunger, thirst).
/// Routes SMAPI time events to each subsystem and owns their tick
/// trackers.
///
/// <para>
/// <see cref="ModEntry"/> only needs to know about this class, not the
/// individual subsystems. The subsystems are internal implementation
/// details.
/// </para>
/// </summary>
public sealed class NeedsSystem
{
    private readonly ContinenceSystem _continence;
    private readonly HungerSystem _hunger;
    private readonly ThirstSystem _thirst;
    private readonly ModConfig _config;
    private readonly LogHelper _log;

    public NeedsSystem(
        StateManager stateManager,
        DataLoader dataLoader,
        ModConfig config,
        ModEventBus eventBus,
        LogHelper log)
    {
        _config = config;
        _log    = log;

        _continence = new ContinenceSystem(
            stateManager, dataLoader, config, eventBus, log);
        _hunger = new HungerSystem(
            stateManager, config, eventBus, log);
        _thirst = new ThirstSystem(
            stateManager, config, eventBus, log);
    }

    public void OnDayStarted()
    {
        if (!_config.Needs.Enabled) return;

        _continence.OnDayStarted();
        _hunger.OnDayStarted();
        _thirst.OnDayStarted();
    }

    public void OnTimeChanged(int newTime)
    {
        if (!_config.Needs.Enabled) return;

        _continence.OnTimeChanged(newTime);
        _hunger.OnTimeChanged(newTime);
        _thirst.OnTimeChanged(newTime);
    }

    public void OnSaveLoaded()
    {
        _continence.ResetTick();
        _hunger.ResetTick();
        _thirst.ResetTick();
    }

    public ContinenceSystem Continence => _continence;
    public HungerSystem Hunger => _hunger;
    public ThirstSystem Thirst => _thirst;
}
