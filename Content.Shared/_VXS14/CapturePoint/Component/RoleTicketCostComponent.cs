namespace Content.Shared.GG.CapturePoint;

/// <summary>
/// This component defines how many tickets a role costs when the player dies.
/// Used in capture point game modes to customize ticket deduction per role.
/// </summary>
[RegisterComponent]
public sealed partial class RoleTicketCostComponent : Component
{
    /// <summary>
    /// Number of tickets to deduct from the team when a player with this role dies.
    /// Default is 1 ticket.
    /// </summary>
    [DataField]
    public int TicketCost = 1;

    /// <summary>
    /// Optional custom message to display when this role dies
    /// </summary>
    [DataField]
    public string? DeathMessage;
}
