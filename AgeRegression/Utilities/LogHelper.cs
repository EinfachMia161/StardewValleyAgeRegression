using StardewModdingAPI;

namespace AgeRegression.Utilities;

/// <summary>
/// Structured logging wrapper around SMAPI's <see cref="IMonitor"/>.
/// Supports a configurable minimum log level so verbose debug output
/// can be suppressed in production without recompiling.
/// </summary>
public sealed class LogHelper
{
    private readonly IMonitor _monitor;
    private readonly LogLevel _minimumLevel;

    /// <param name="monitor">The SMAPI monitor to write to.</param>
    /// <param name="minimumLevel">
    /// Messages below this level are silently discarded.
    /// Defaults to <see cref="LogLevel.Debug"/>.
    /// </param>
    public LogHelper(IMonitor monitor, LogLevel minimumLevel = LogLevel.Debug)
    {
        _monitor      = monitor;
        _minimumLevel = minimumLevel;
    }

    /// <summary>Logs a trace-level message (most verbose).</summary>
    public void Trace(string message, params object?[] args) =>
        Write(LogLevel.Trace, message, args);

    /// <summary>Logs a debug-level message.</summary>
    public void Debug(string message, params object?[] args) =>
        Write(LogLevel.Debug, message, args);

    /// <summary>Logs an informational message.</summary>
    public void Info(string message, params object?[] args) =>
        Write(LogLevel.Info, message, args);

    /// <summary>Logs a warning.</summary>
    public void Warn(string message, params object?[] args) =>
        Write(LogLevel.Warn, message, args);

    /// <summary>Logs a non-fatal error.</summary>
    public void Error(string message, params object?[] args) =>
        Write(LogLevel.Error, message, args);

    /// <summary>
    /// Logs an exception with a descriptive context message at Error level.
    /// </summary>
    public void Exception(string context, Exception ex) =>
        _monitor.Log($"[AgeRegression] {context}: {ex}", LogLevel.Error);

    // -------------------------------------------------------------------------

    private void Write(LogLevel level, string message, object?[] args)
    {
        if (level < _minimumLevel)
            return;

        var formatted = args.Length > 0
            ? string.Format(message, args)
            : message;

        _monitor.Log($"[AgeRegression] {formatted}", level);
    }
}
