using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Content.Shared.Antag;

namespace Content.Shared.GG.CapturePoint;

public abstract class SharedGGSyndyTeamSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        // Keep this subscription to handle state attempts for GGSyndyTeamComponent.
        SubscribeLocalEvent<GGSyndyTeamComponent, ComponentGetStateAttemptEvent>(OnSyndyCompGetStateAttempt);

        // Only subscribe to GGSyndyTeamComponent startup for DirtySyndyComps.
        SubscribeLocalEvent<GGSyndyTeamComponent, ComponentStartup>(DirtySyndyComps);

        // Remove redundant subscription for ShowAntagIconsComponent.
        // Antag-specific logic should be handled elsewhere if needed.
    }

    private void OnSyndyCompGetStateAttempt(EntityUid uid, GGSyndyTeamComponent comp, ref ComponentGetStateAttemptEvent args)
    {
        args.Cancelled = !CanGetState(args.Player);
    }

    private bool CanGetState(ICommonSession? player)
    {
        // Allow state retrieval if player has GGSyndyTeamComponent or ShowAntagIconsComponent.
        if (player?.AttachedEntity is not { } uid)
            return true;

        return HasComp<GGSyndyTeamComponent>(uid);
    }

    private void DirtySyndyComps(EntityUid someUid, GGSyndyTeamComponent someComp, ComponentStartup ev)
    {
        // Iterate over all entities with GGSyndyTeamComponent and mark them dirty.
        var SyndyComps = AllEntityQuery<GGSyndyTeamComponent>();
        while (SyndyComps.MoveNext(out var uid, out var comp))
        {
            Dirty(uid, comp);
        }
    }
}
