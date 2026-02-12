using Content.Server.Chat.Managers;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.GG.CapturePoint;
using Content.Server.GG.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.RoundEnd;
using Content.Shared.Chat;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.GG.CapturePoint;
using Content.Shared.Interaction.Events;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Players;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.GameTicking.Rules;

/// <summary>
/// This handles logic and interactions related to <see cref="SendToLobbyOnDeathRuleComponent"/>
/// </summary>
public sealed class SendToLobbyOnDeathRuleSystem : GameRuleSystem<SendToLobbyOnDeathRuleComponent>
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly SharedGGCapturePointSystem _capturePointSystem = default!;
    [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SuicideEvent>(OnSuicide);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnSuicide(SuicideEvent ev)
    {
        if (!TryComp<ActorComponent>(ev.Victim, out var actor))
           return;

        var query = EntityQueryEnumerator<SendToLobbyOnDeathRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var lobbyRule, out var rule))
        {
            if (!GameTicker.IsGameRuleActive(uid, rule))
                continue;

            if (lobbyRule.AlwaysSendToLobby)
            {
                SendPlayerToLobby(actor.PlayerSession);
            }
        }
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (!TryComp<ActorComponent>(args.Target, out var actor))
            return;

        var query = EntityQueryEnumerator<SendToLobbyOnDeathRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var lobbyRule, out var rule))
        {
            if (!GameTicker.IsGameRuleActive(uid, rule))
                continue;

            if (lobbyRule.AlwaysSendToLobby)
            {
                SendPlayerToLobby(actor.PlayerSession);
            }
        }
    }

    /// <summary>
    /// Sends a player to the lobby immediately after death
    /// </summary>
    public void SendPlayerToLobby(ICommonSession session)
    {
        // Remove the player from any mind they might have
        if (session.GetMind() is { } mind && TryComp<MindComponent>(mind, out var mindComp))
        {
            if (mindComp.OwnedEntity.HasValue)
            {
                var playerEntity = mindComp.OwnedEntity.Value;

                // Check team affiliation and deduct tickets
                DeductTicketsForDeath(playerEntity);

                QueueDel(playerEntity);
            }

            // Transfer the mind to null entity to detach it
            _mindSystem.TransferTo(mind, null, mind: mindComp);
        }

        // Send the player to lobby by raising the network event
        RaiseNetworkEvent(new TickerJoinLobbyEvent(), session.Channel);

        // Send a notification to the player
        var msg = Loc.GetString("rule-sent-to-lobby-on-death");
        var wrappedMsg = Loc.GetString("chat-manager-server-wrap-message", ("message", msg));
        _chatManager.ChatMessageToOne(ChatChannel.Server, msg, wrappedMsg, EntityUid.Invalid, false, session.Channel, Color.LimeGreen);
    }

    /// <summary>
    /// Deducts tickets from the appropriate team when a player dies
    /// </summary>
    private void DeductTicketsForDeath(EntityUid playerEntity)
    {
        // Check if this is a CapturePoint game mode
        var capturePointQuery = EntityQueryEnumerator<CapturePointGameRuleComponent, GameRuleComponent, CaptureTicketsComponent>();

        while (capturePointQuery.MoveNext(out var ruleUid, out var captureRule, out var gameRule, out var tickets))
        {
            if (!GameTicker.IsGameRuleActive(ruleUid, gameRule))
                continue;

            // Get ticket cost for this role (default to 1 if not specified)
            var ticketCost = 1;
            string? deathMessage = null;

            if (TryComp<RoleTicketCostComponent>(playerEntity, out var roleCost))
            {
                ticketCost = roleCost.TicketCost;
                deathMessage = roleCost.DeathMessage;
            }

            // Check team affiliation
            if (HasComp<GGSolfedTeamComponent>(playerEntity))
            {
                captureRule.SoledTeamPoints = Math.Max(0, captureRule.SoledTeamPoints - ticketCost);
                tickets.SolfedTickets = captureRule.SoledTeamPoints;
                Dirty(ruleUid, tickets);

                var logMessage = deathMessage ?? $"Solfed player died → -{ticketCost} ticket(s) → {captureRule.SoledTeamPoints}";
                Log.Info($"[SendToLobbyOnDeath] {logMessage}");
            }
            else if (HasComp<GGSyndyTeamComponent>(playerEntity))
            {
                captureRule.SyndyTeamPoints = Math.Max(0, captureRule.SyndyTeamPoints - ticketCost);
                tickets.SyndyTickets = captureRule.SyndyTeamPoints;
                Dirty(ruleUid, tickets);

                var logMessage = deathMessage ?? $"Syndicate player died → -{ticketCost} ticket(s) → {captureRule.SyndyTeamPoints}";
                Log.Info($"[SendToLobbyOnDeath] {logMessage}");
            }

            // Check for victory conditions
            if (captureRule.SoledTeamPoints <= 0)
            {
                captureRule.Victor = "Syndy";
                // Trigger round end
                _roundEndSystem.EndRound(TimeSpan.FromSeconds(10f));
            }
            else if (captureRule.SyndyTeamPoints <= 0)
            {
                captureRule.Victor = "Solfed";
                // Trigger round end
                _roundEndSystem.EndRound(TimeSpan.FromSeconds(10f));
            }
        }
    }
}
