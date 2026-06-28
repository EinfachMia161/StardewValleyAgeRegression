using AgeRegression.Data;
using AgeRegression.State;

namespace AgeRegression.Api;

/// <summary>
/// Public API exposed to other mods via SMAPI's mod registry.
/// Other mods retrieve this interface with:
/// <code>
/// var api = helper.ModRegistry
///     .GetApi&lt;IAgeRegressionApi&gt;("mia.AgeRegression");
/// </code>
///
/// <para>
/// Versioning: this interface will not have breaking changes within a
/// major version. New methods will be added as the mod grows. Callers
/// should null-check optional members if they target multiple mod
/// versions.
/// </para>
/// </summary>
public interface IAgeRegressionApi
{
    // -------------------------------------------------------------------------
    // Regression stage queries
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the ID of the local player's current regression stage,
    /// or <c>null</c> if no save is loaded.
    /// </summary>
    string? GetCurrentStageId();

    /// <summary>
    /// Returns all available regression stage definitions, ordered by
    /// stage order.
    /// </summary>
    IReadOnlyList<RegressionStageData> GetAllStages();

    /// <summary>
    /// Returns <c>true</c> if the local player is currently at any
    /// regressed stage.
    /// </summary>
    bool IsRegressed();

    // -------------------------------------------------------------------------
    // Diaper state queries
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns <c>true</c> if the local player is currently wearing a
    /// diaper.
    /// </summary>
    bool IsWearingDiaper();

    /// <summary>
    /// Returns the current diaper condition as a lowercase string:
    /// <c>"clean"</c>, <c>"damp"</c>, <c>"wet"</c>, or
    /// <c>"soaked"</c>. Returns <c>"none"</c> if no diaper is equipped.
    /// </summary>
    string GetDiaperCondition();

    // -------------------------------------------------------------------------
    // Comfort queries
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the player's current comfort score (0.0–MaxComfort).
    /// Returns 0 if no save is loaded.
    /// </summary>
    float GetComfortScore();

    // -------------------------------------------------------------------------
    // Stage mutation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Attempts to set the local player's regression stage to the
    /// given ID. Returns <c>false</c> if the stage ID is invalid or
    /// no save is loaded.
    /// </summary>
    bool TrySetStage(string stageId);

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    /// <summary>
    /// Raised when the local player's regression stage changes.
    /// </summary>
    event EventHandler<RegressionStageChangedApiEventArgs>?
        RegressionStageChanged;

    /// <summary>
    /// Raised when the local player's diaper state changes.
    /// </summary>
    event EventHandler<DiaperStateChangedApiEventArgs>?
        DiaperStateChanged;
}

/// <summary>
/// Event args for
/// <see cref="IAgeRegressionApi.RegressionStageChanged" />.
/// </summary>
public sealed class RegressionStageChangedApiEventArgs : EventArgs
{
    /// <summary>The stage ID before the change.</summary>
    public string PreviousStageId { get; init; } = string.Empty;

    /// <summary>The stage ID after the change.</summary>
    public string NewStageId { get; init; } = string.Empty;
}

/// <summary>
/// Event args for
/// <see cref="IAgeRegressionApi.DiaperStateChanged" />.
/// </summary>
public sealed class DiaperStateChangedApiEventArgs : EventArgs
{
    /// <summary>Whether the player is now wearing a diaper.</summary>
    public bool IsWearingDiaper { get; init; }

    /// <summary>The new diaper condition ID.</summary>
    public string ConditionId { get; init; } = string.Empty;
}
