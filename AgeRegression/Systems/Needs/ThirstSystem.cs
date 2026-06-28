using AgeRegression.Config;
using AgeRegression.Events;
using AgeRegression.State;
using AgeRegression.Utilities;

namespace AgeRegression.Systems.Needs;

public sealed class ThirstSystem
{
    public ThirstSystem(
        StateManager stateManager,
        ModConfig config,
        ModEventBus eventBus,
        LogHelper log)
    {
    }

    public void OnDayStarted() { }

    public void OnTimeChanged(int newTime) { }

    public void ResetTick() { }
}
