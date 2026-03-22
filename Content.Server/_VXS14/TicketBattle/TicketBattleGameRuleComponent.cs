using Robust.Shared.Player;

namespace Content.Server.VXS14.TicketBattle;

/// <summary>
/// Game rule component for the Ticket Battle mode.
/// Two teams (Solfed vs Syndy) start with a fixed pool of tickets.
/// Each player death deducts that role's <see cref="Content.Shared.GG.CapturePoint.RoleTicketCostComponent.TicketCost"/> from the team.
/// The first team to reach 0 tickets loses.
/// </summary>
[RegisterComponent]
public sealed partial class TicketBattleGameRuleComponent : Component
{
    /// <summary>
    /// Starting ticket count for each team.
    /// </summary>
    [DataField]
    public int InitialTickets = 100;

    /// <summary>
    /// Current Solfed ticket count.
    /// </summary>
    public int SolfedTickets;

    /// <summary>
    /// Current Syndy ticket count.
    /// </summary>
    public int SyndyTickets;

    /// <summary>
    /// Set to the winning team name when the round ends.
    /// </summary>
    public string? Victor;

    /// <summary>
    /// Tracks the first job each session chose so that respawns keep the same team assignment.
    /// </summary>
    [DataField]
    public Dictionary<ICommonSession, string> PlayerFirstJob = new();
}
