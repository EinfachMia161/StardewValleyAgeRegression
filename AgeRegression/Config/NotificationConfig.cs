namespace AgeRegression.Config;

/// <summary>
/// Configuration for the player notification system.
/// </summary>
public sealed class NotificationConfig
{
    /// <summary>Whether notifications are enabled.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Minimum comfort change magnitude to trigger a notification.
    /// Changes smaller than this are considered minor and not notified.
    /// </summary>
    public float MinComfortChangeForNotification { get; set; } = 10f;

    /// <summary>
    /// Minimum mood tier change to trigger a notification.
    /// Set to 1 to notify on any mood change, 2 to skip adjacent tiers, etc.
    /// </summary>
    public int MinMoodTierChangeForNotification { get; set; } = 1;
}