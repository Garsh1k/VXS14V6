namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// This component controls periodic ticket display in chat for capture point game modes.
/// </summary>
[RegisterComponent]
public sealed partial class TicketDisplayComponent : Component
{
    /// <summary>
    /// Whether to display tickets in chat periodically
    /// </summary>
    [DataField]
    public bool DisplayEnabled = true;

    /// <summary>
    /// Interval between ticket displays in seconds (default: 60 seconds = 1 minute)
    /// </summary>
    [DataField]
    public int DisplayInterval = 60;

    /// <summary>
    /// Last time tickets were displayed
    /// </summary>
    [DataField]
    public TimeSpan LastDisplayTime = TimeSpan.Zero;
}
