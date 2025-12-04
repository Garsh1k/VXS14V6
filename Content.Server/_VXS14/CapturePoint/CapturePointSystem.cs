using System.Collections.Generic;
using Content.Shared.GG.CapturePoint;
using Robust.Shared.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using System.Linq;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Content.Shared.Chat;
using Content.Server.Chat.Managers;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server.GG.CapturePoint;

public sealed class GGCapturePointSystem : SharedGGCapturePointSystem
{
    private const float CaptureRate = 20f; // Progress per second when capturing
    private const float DecayRate = 20f; // Progress per second when decaying

    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GGCapturePointComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<GGCapturePointComponent, EndCollideEvent>(OnEndCollide);

        SubscribeLocalEvent<GGCapturePointComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<GGCapturePointComponent, ComponentStartup>(OnCapturePointStartup);
        SubscribeLocalEvent<GGCapturePointComponent, ComponentShutdown>(OnCapturePointShutdown);
    }


    private void OnCapturePointStartup(EntityUid uid, GGCapturePointComponent component, ComponentStartup args)
    {
        component.PointUid = uid;
        component.CurrentPointProgression = 0f;
        component.Leader = "None";
        component.CollidingEntities.Clear();
    }

    private void OnCapturePointShutdown(EntityUid uid, GGCapturePointComponent component, ComponentShutdown args)
    {
        component.CurrentPointProgression = 0f;
        component.Leader = "None";
        component.CollidingEntities.Clear();
    }

    private void OnStartCollide(EntityUid uid, GGCapturePointComponent component, ref StartCollideEvent args)
    {
            //         Logger.Error($"{component.Leader}");
            // Logger.Error($"{component.CurrentPointProgression }");
        if (!args.OtherFixture.Hard)
            return;

        component.CollidingEntities.Add(args.OtherEntity);
    }

    private void OnEndCollide(EntityUid uid, GGCapturePointComponent component, ref EndCollideEvent args)
    {
            //         Logger.Error($"{component.Leader}");
            // Logger.Error($"{component.CurrentPointProgression }");
        component.CollidingEntities.Remove(args.OtherEntity);
    }

    public override void Update(float frameTime)
    {

        foreach (var (capturePoint, transform) in EntityQuery<GGCapturePointComponent, TransformComponent>())
        {
            ProcessCapturePoint(capturePoint, frameTime);
        }

    }

    private void ProcessCapturePoint(GGCapturePointComponent capturePoint, float frameTime)
    {
        HashSet<string> teams = new();

        foreach (var entity in capturePoint.CollidingEntities)
        {
            if (EntityManager.HasComponent<GGSyndyTeamComponent>(entity))
            {

                teams.Add("Syndy");
            }
            else if (EntityManager.HasComponent<GGSolfedTeamComponent>(entity))
            {
                teams.Add("Solfed");
            }
        }

        if (teams.Count == 0 || teams.Count > 1)
        {
            //DecayPoint(capturePoint, frameTime);
            return;
        }

        var team = teams.First();
        capturePoint.Team = team;
        // Logger.Error($"Team: {team}"); // Log the full team name.
        // Logger.Error($"capturePoint.Leader = {capturePoint.Leader}");

        if (capturePoint.Leader != team)
        {
            if(capturePoint.Leader == "None" )
                CapturePoint(capturePoint, team, frameTime);
            else
            {
                // Logger.Error($"NeutralizePoint");
                NeutralizePoint(capturePoint, frameTime);
            }
        }
        else
        {
            CapturePoint(capturePoint, team, frameTime);
        }

        if(capturePoint.CurrentPointProgression == 100)
        {
            if(capturePoint.Leader == team)
            {
                if(capturePoint.Captured == false)
                {
                    var message = Loc.GetString("gg-capturepoint-team-" + capturePoint.Leader.ToLower(), ("point", capturePoint.PointName));
                    var color = Color.Red;
                    if(capturePoint.Leader == "Solfed")
                        color = Color.Blue;
                    _chatManager.ChatMessageToAll(ChatChannel.Server, message, message, capturePoint.PointUid, false, true, color);
                    foreach (var pSession in Filter.GetAllPlayers())
                    {
                        // Audio.PlayGlobal("/Audio/GG/CapturePoint/captured.ogg", pSession);
                    }

                    capturePoint.Captured = true;
                }
            }
        }
        else if (capturePoint.CurrentPointProgression == 0)
        {
            capturePoint.Captured = false;
        }
    }

    public void CapturePoint(GGCapturePointComponent capturePoint, string team, float frameTime)
    {

        capturePoint.CurrentPointProgression = MathF.Min(100, capturePoint.CurrentPointProgression + CaptureRate * frameTime);
        // Logger.Error($"{capturePoint.CurrentPointProgression }");
        if (capturePoint.CurrentPointProgression >= 100)
        {
            capturePoint.Leader = team;
        }
        Dirty(capturePoint.Owner, capturePoint);

    }

    public Dictionary<string, float> GetTeamPoints()
    {
        var points = new Dictionary<string, float> { { "Syndy", 0f }, { "Solfed", 0f } };

        foreach (var (capturePoint, _) in EntityQuery<GGCapturePointComponent, TransformComponent>())
        {
            if (capturePoint.Leader == "Syndy")
            {
                points["Syndy"]++;
            }
            else if (capturePoint.Leader == "Solfed")
            {
                points["Solfed"]++;
            }
        }

        return points;
    }


    // private void DecayPoint(GGCapturePointComponent capturePoint, float frameTime)
    // {
    //     capturePoint.CurrentPointProgression = MathF.Max(0, capturePoint.CurrentPointProgression - DecayRate * frameTime);
    //     if (capturePoint.CurrentPointProgression == 0)
    //     {
    //         capturePoint.Leader = "None";
    //     }
    //     Dirty(capturePoint);
    // }

    // private void NeutralizePoint(GGCapturePointComponent capturePoint, float frameTime)
    // {
    //     capturePoint.CurrentPointProgression = MathF.Max(0, capturePoint.CurrentPointProgression - DecayRate * frameTime);
    //     if (capturePoint.CurrentPointProgression == 0)
    //     {
    //         capturePoint.Leader = "None";
    //     }
    //     Dirty(capturePoint);
    // }

    // private void CapturePoint(GGCapturePointComponent capturePoint, string team, float frameTime)
    // {
    //     Logger.Error($"CApturing");

    //     capturePoint.CurrentPointProgression = MathF.Min(100, capturePoint.CurrentPointProgression + CaptureRate * frameTime);
    //     Logger.Error($"{capturePoint.CurrentPointProgression }");
    //     if (capturePoint.CurrentPointProgression >= 100)
    //     {
    //         capturePoint.Leader = team;

    //     Dirty(capturePoint);
    // }

}

[ByRefEvent]
public readonly record struct PointCapturedEvent(string Name, string Leader);
