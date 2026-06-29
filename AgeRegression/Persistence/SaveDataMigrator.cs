using AgeRegression.Utilities;

namespace AgeRegression.Persistence;

/// <summary>
/// Applies sequential schema migrations to a <see cref="SaveDataModel"/>
/// loaded from an older save file.
///
/// <para>
/// To add a migration:
/// <list type="number">
///   <item>Increment <see cref="CurrentSchemaVersion"/>.</item>
///   <item>Add a private method named <c>MigrateVxToVy</c>.</item>
///   <item>Add a <c>case</c> to the switch in
///   <see cref="Migrate"/>.</item>
/// </list>
/// </summary>
public sealed class SaveDataMigrator
{
    /// <summary>
    /// The schema version this build of the mod expects.
    /// Must match <see cref="SaveDataModel.SchemaVersion"/> after
    /// migration completes.
    /// </summary>
    public const int CurrentSchemaVersion = 3;

    private readonly LogHelper _log;

    public SaveDataMigrator(LogHelper log)
    {
        _log = log;
    }

    /// <summary>
    /// Migrates <paramref name="data"/> from its current schema version
    /// to <see cref="CurrentSchemaVersion"/>. Modifies the object in
    /// place.
    /// </summary>
    /// <returns>
    /// <c>true</c> if any migrations were applied; <c>false</c> if the
    /// data was already at the current version.
    /// </returns>
    public bool Migrate(SaveDataModel data)
    {
        if (data.SchemaVersion == CurrentSchemaVersion)
            return false;

        if (data.SchemaVersion > CurrentSchemaVersion)
        {
            _log.Warn(
                "Save data schema v{0} is newer than mod version v{1}. " +
                "Skipping migration.",
                data.SchemaVersion, CurrentSchemaVersion);
            return false;
        }

        _log.Info("Migrating save data from schema v{0} to v{1}.",
            data.SchemaVersion, CurrentSchemaVersion);

        while (data.SchemaVersion < CurrentSchemaVersion)
        {
            switch (data.SchemaVersion)
            {
                case 1:
                    MigrateV1ToV2(data);
                    break;
                case 2:
                    MigrateV2ToV3(data);
                    break;

                default:
                    _log.Warn(
                        "No migration defined from schema v{0}. Stopping.",
                        data.SchemaVersion);
                    return true;
            }
        }

        _log.Info("Migration complete. Schema is now v{0}.",
            data.SchemaVersion);
        return true;
    }

    // -------------------------------------------------------------------------
    // Migration steps
    // -------------------------------------------------------------------------

    /// <summary>
    /// v1 → v2: Added needs state (continence, hunger, thirst).
    /// All new fields have safe defaults already defined on
    /// <see cref="SaveDataModel"/>, so deserialization from a v1 JSON
    /// blob produces correct defaults automatically. We only need to
    /// bump the version number here.
    /// </summary>
    private void MigrateV1ToV2(SaveDataModel data)
    {
        _log.Debug(
            "Applying migration v1 → v2: " +
            "initializing needs state with defaults.");
        data.SchemaVersion = 2;
    }

    /// <summary>
    /// v2 → v3: Added care state.
    /// All new fields have safe defaults, so we only need to bump the version.
    /// </summary>
    private void MigrateV2ToV3(SaveDataModel data)
    {
        _log.Debug(
            "Applying migration v2 → v3: " +
            "initializing care state with defaults.");
        data.SchemaVersion = 3;
    }
}