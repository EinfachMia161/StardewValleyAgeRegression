namespace AgeRegression.Config;

/// <summary>
/// Settings for the in-game status HUD overlay.
/// </summary>
public sealed class HudConfig
{
    /// <summary>
    /// Master switch for the status HUD. When <c>false</c>, the HUD
    /// is not rendered regardless of other settings.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Horizontal screen position in pixels from the left edge.
    /// </summary>
    public int PositionX { get; set; } = 16;

    /// <summary>
    /// Vertical screen position in pixels from the top edge.
    /// </summary>
    public int PositionY { get; set; } = 100;

    /// <summary>
    /// Render scale multiplier. 1.0 is default size.
    /// </summary>
    public float Scale { get; set; } = 1.0f;

    /// <summary>
    /// When <c>true</c>, diaper-related information is shown in the HUD.
    /// </summary>
    public bool ShowDiaperInfo { get; set; } = true;

    /// <summary>
    /// When <c>true</c>, needs-related information is shown in the HUD.
    /// </summary>
    public bool ShowNeedsInfo { get; set; } = true;

    /// <summary>
    /// When <c>true</c>, regression-related information is shown in the HUD.
    /// </summary>
    public bool ShowRegressionInfo { get; set; } = true;
}