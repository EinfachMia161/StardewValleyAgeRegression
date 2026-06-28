namespace AgeRegression.Data;

/// <summary>
/// Abstraction over the source of mod data assets.
/// Allows <see cref="DataLoader"/> to be tested in isolation and
/// makes migration to SMAPI's <c>IGameContentHelper</c> in a future
/// phase a drop-in replacement rather than a refactor.
///
/// <para>
/// Phase 1 implementation: <see cref="FileSystemAssetProvider"/> reads
/// directly from disk.
/// Future: A <c>SmapiAssetProvider</c> will route through
/// <c>helper.GameContent.Load&lt;T&gt;</c> so Content Patcher can
/// override assets.
/// </para>
///
/// <para>
/// Design contract: all relative paths passed to <see cref="Load{T}"/>
/// and <see cref="EnumerateFiles"/> are relative to the mod root
/// directory. Absolute paths returned by <see cref="EnumerateFiles"/>
/// are passed directly to <see cref="LoadRaw"/>.
/// </para>
/// </summary>
public interface IAssetProvider
{
    /// <summary>
    /// Loads and deserializes a JSON asset at the given relative path.
    /// </summary>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <param name="relativePath">
    /// Path relative to the mod root.
    /// Example: <c>assets/data/regression-stages.json</c>.
    /// </param>
    /// <returns>
    /// The deserialized object, or <c>null</c> if the file does not
    /// exist or cannot be deserialized.
    /// </returns>
    T? Load<T>(string relativePath) where T : class;

    /// <summary>
    /// Loads the raw text content of a file at the given absolute path.
    ///
    /// <para>
    /// Used for files that require custom parsing (e.g. dialogue packs
    /// discovered via <see cref="EnumerateFiles"/> and event scripts).
    /// Plain text files cannot be loaded via SMAPI's
    /// <c>IModContentHelper.Load&lt;string&gt;</c> because that method
    /// requires XNA content pipeline compilation.
    /// </para>
    /// </summary>
    /// <param name="absolutePath">
    /// The absolute file path as returned by
    /// <see cref="EnumerateFiles"/>. Implementations that return
    /// absolute paths from <see cref="EnumerateFiles"/> must accept
    /// those same paths here.
    /// </param>
    /// <returns>
    /// The raw file content as a string, or <c>null</c> if the file
    /// cannot be read.
    /// </returns>
    string? LoadRaw(string absolutePath);

    /// <summary>
    /// Returns all file paths matching the given pattern within a
    /// directory. Used by <see cref="DataLoader"/> to discover dialogue
    /// pack files and event scripts. Returned paths are passed back to
    /// <see cref="LoadRaw"/> for reading.
    /// </summary>
    /// <param name="relativeDirectory">
    /// Directory path relative to the mod root.
    /// </param>
    /// <param name="searchPattern">
    /// Glob pattern, e.g. <c>*.json</c>.
    /// </param>
    /// <param name="recursive">
    /// Whether to search subdirectories.
    /// </param>
    IEnumerable<string> EnumerateFiles(
        string relativeDirectory,
        string searchPattern = "*.json",
        bool recursive = true);
}
