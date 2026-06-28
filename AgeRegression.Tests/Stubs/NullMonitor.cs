using StardewModdingAPI;
using StardewModdingAPI.Framework.Logging;

namespace AgeRegression.Tests;

/// <summary>
/// No-op <see cref="IMonitor"/> for unit tests. Discards all log output.
/// </summary>
internal sealed class NullMonitor : IMonitor
{
    public static readonly NullMonitor Instance = new();

    public bool IsVerbose => false;
    public void Log(string message, LogLevel level = LogLevel.Debug) { }
    public void LogOnce(string message, LogLevel level = LogLevel.Debug) { }
    public void VerboseLog(string message) { }
    public void VerboseLog(ref VerboseLogStringHandler message) { }
}
