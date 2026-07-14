namespace AgeRegression.Data;

/// <summary>
/// Marker interface for a single, immutable unlock condition (a value
/// object carrying only the data needed to describe one requirement, such
/// as a minimum year or a required mail flag).
///
/// <para>
/// Implementations are pure data and must not reference game state or
/// Stardew APIs. All interpretation lives in
/// <see cref="Items.UnlockConditionEvaluator"/>, which depends on
/// <see cref="Items.IGameStateProvider"/>.
/// </para>
/// </summary>
public interface IUnlockCondition
{
}
