using AgeRegression.Data;

namespace AgeRegression.Items;

/// <summary>
/// Interprets <see cref="IUnlockCondition"/> instances against the current
/// game state. This is the only place that knows how to evaluate a
/// condition; condition classes themselves are immutable data only.
///
/// <para>
/// Depends solely on <see cref="IGameStateProvider"/> — no Stardew API
/// calls and no item, shop, or rendering logic.
/// </para>
/// </summary>
public sealed class UnlockConditionEvaluator
{
    private readonly IGameStateProvider _gameState;

    public UnlockConditionEvaluator(IGameStateProvider gameState)
    {
        _gameState = gameState;
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="condition"/> is currently
    /// satisfied. Unknown condition types evaluate to <c>false</c> (reserved
    /// for future implementation).
    /// </summary>
    public bool Evaluate(IUnlockCondition condition) => condition switch
    {
        YearCondition c       => _gameState.CurrentYear >= c.RequiredYear,
        SeasonCondition c     => string.Equals(
            _gameState.CurrentSeason, c.RequiredSeason,
            StringComparison.OrdinalIgnoreCase),
        FriendshipCondition c => (_gameState.GetFriendshipPoints(c.NpcName) ?? -1)
            >= c.RequiredPoints,
        MailFlagCondition c   => _gameState.HasReceivedMail(c.MailFlag),
        _                      => false
    };
}
