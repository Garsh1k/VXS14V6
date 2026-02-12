namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// This is used for gamemodes that send players to lobby when they die instead of respawning them.
/// </summary>
[RegisterComponent, Access(typeof(SendToLobbyOnDeathRuleSystem))]
public sealed partial class SendToLobbyOnDeathRuleComponent : Component
{
    /// <summary>
    /// Whether or not we want to send everyone who dies to the lobby
    /// </summary>
    [DataField]
    public bool AlwaysSendToLobby = true;
}
