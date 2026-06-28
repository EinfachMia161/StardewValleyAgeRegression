using AgeRegression.Data;
using AgeRegression.Utilities;

namespace AgeRegression.Systems;

/// <summary>
/// <see cref="IStatModifierProvider"/> that contributes stat modifiers
/// based on furniture proximity.
///
/// <para>
/// Currently furniture proximity only affects comfort (via
/// <see cref="FurnitureProximitySystem"/>), not stats directly.
/// This provider is a placeholder for future furniture that grants
/// stat bonuses (e.g. a crib that reduces energy drain while resting).
/// </para>
/// </summary>
public sealed class FurnitureStatModifierProvider : IStatModifierProvider
{
    private readonly FurnitureProximitySystem _proximitySystem;
    private readonly DataLoader _dataLoader;
    private readonly LogHelper _log;

    public string ProviderId => "furniture_proximity";

    public FurnitureStatModifierProvider(
        FurnitureProximitySystem proximitySystem,
        DataLoader dataLoader,
        LogHelper log)
    {
        _proximitySystem = proximitySystem;
        _dataLoader      = dataLoader;
        _log             = log;
    }

    public StatModifierContribution GetContribution()
    {
        return StatModifierContribution.Identity;
    }
}
