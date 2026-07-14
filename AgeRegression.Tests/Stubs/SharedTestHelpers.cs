using System;
using System.Collections.Generic;
using System.Linq;
using AgeRegression.Data;

namespace AgeRegression.Tests;

/// <summary>
/// An <see cref="IAssetProvider"/> that returns null for every call,
/// simulating missing asset files. Forces <see cref="DataLoader"/> to
/// use its built-in defaults.
/// </summary>
internal sealed class EmptyAssetProvider : IAssetProvider
{
    public T? Load<T>(string relativePath) where T : class => null;

    public string? LoadRaw(string absolutePath) => null;

    public IEnumerable<string> EnumerateFiles(
        string relativeDirectory,
        string searchPattern = "*.json",
        bool recursive = true) => Enumerable.Empty<string>();
}

/// <summary>
/// An <see cref="IAssetProvider"/> backed by an in-memory dictionary
/// of JSON strings. Used to inject specific test data into
/// <see cref="DataLoader"/> without touching the file system.
/// </summary>
internal sealed class InMemoryAssetProvider : IAssetProvider
{
    private readonly Dictionary<string, string> _files;

    public InMemoryAssetProvider(Dictionary<string, string> files)
    {
        _files = files;
    }

    public T? Load<T>(string relativePath) where T : class
    {
        if (!_files.TryGetValue(relativePath, out var json))
            return null;
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, AgeRegression.Data.AssetJson.Settings);
    }

    public string? LoadRaw(string absolutePath) =>
        _files.TryGetValue(absolutePath, out var raw) ? raw : null;

    public IEnumerable<string> EnumerateFiles(
        string relativeDirectory,
        string searchPattern = "*.json",
        bool recursive = true) => Enumerable.Empty<string>();
}