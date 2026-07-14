using AgeRegression.Utilities;
using Newtonsoft.Json;

namespace AgeRegression.Data;

/// <summary>
/// <see cref="IAssetProvider"/> implementation that reads assets directly
/// from the file system.
///
/// <para>
/// This is the current implementation used in all phases. A future
/// <c>SmapiAssetProvider</c> will be added alongside this one when
/// Content Patcher integration is needed for patchable runtime assets.
/// The two providers can coexist: file system for mod-internal defaults,
/// SMAPI for patchable runtime assets.
/// </para>
/// </summary>
public sealed class FileSystemAssetProvider : IAssetProvider
{
    private readonly string _modRootPath;
    private readonly LogHelper _log;

    /// <param name="modRootPath">
    /// Absolute path to the mod's root directory.
    /// Typically <c>helper.DirectoryPath</c>.
    /// </param>
    /// <param name="log">The mod logger.</param>
    public FileSystemAssetProvider(string modRootPath, LogHelper log)
    {
        _modRootPath = modRootPath;
        _log         = log;
    }

    /// <inheritdoc />
    public T? Load<T>(string relativePath) where T : class
    {
        var fullPath = Path.Combine(_modRootPath, relativePath);
        var raw      = LoadRaw(fullPath);
        if (raw is null)
            return null;

        try
        {
            return JsonConvert.DeserializeObject<T>(raw, AssetJson.Settings);
        }
        catch (Exception ex)
        {
            _log.Exception($"Failed to deserialize asset '{relativePath}'", ex);
            return null;
        }
    }

    /// <inheritdoc />
    public string? LoadRaw(string absolutePath)
    {
        if (!File.Exists(absolutePath))
        {
            // Use a relative path in the log message for readability
            var display = absolutePath.StartsWith(
                    _modRootPath, StringComparison.OrdinalIgnoreCase)
                ? absolutePath[_modRootPath.Length..]
                    .TrimStart(Path.DirectorySeparatorChar,
                               Path.AltDirectorySeparatorChar)
                : absolutePath;

            _log.Warn("Asset not found: {0}", display);
            return null;
        }

        try
        {
            return File.ReadAllText(absolutePath);
        }
        catch (Exception ex)
        {
            _log.Exception($"Failed to read file '{absolutePath}'", ex);
            return null;
        }
    }

    /// <inheritdoc />
    public IEnumerable<string> EnumerateFiles(
        string relativeDirectory,
        string searchPattern = "*.json",
        bool recursive = true)
    {
        var fullDir = Path.Combine(_modRootPath, relativeDirectory);

        if (!Directory.Exists(fullDir))
        {
            _log.Warn("Asset directory not found: {0}", relativeDirectory);
            return Enumerable.Empty<string>();
        }

        var option = recursive
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        return Directory.GetFiles(fullDir, searchPattern, option);
    }
}
