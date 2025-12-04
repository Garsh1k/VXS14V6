using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Content.Shared.Antag;

namespace Content.Shared.GG.CapturePoint;

public abstract class SharedGGSolfedTeamSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        // Keep this subscription to handle state attempts for GGSolfedTeamComponent.
        SubscribeLocalEvent<GGSolfedTeamComponent, ComponentGetStateAttemptEvent>(OnSolfedCompGetStateAttempt);

        // Only subscribe to GGSolfedTeamComponent startup for DirtySolfedComps.
        SubscribeLocalEvent<GGSolfedTeamComponent, ComponentStartup>(DirtySolfedComps);

        // Remove redundant subscription for ShowAntagIconsComponent.
        // Antag-specific logic should be handled elsewhere if needed.
    }

    private void OnSolfedCompGetStateAttempt(EntityUid uid, GGSolfedTeamComponent comp, ref ComponentGetStateAttemptEvent args)
    {
        args.Cancelled = !CanGetState(args.Player);
    }

    private bool CanGetState(ICommonSession? player)
    {
        // Allow state retrieval if player has GGSolfedTeamComponent or ShowAntagIconsComponent.
        if (player?.AttachedEntity is not { } uid)
            return true;

        return HasComp<GGSolfedTeamComponent>(uid);
    }

    private void DirtySolfedComps(EntityUid someUid, GGSolfedTeamComponent someComp, ComponentStartup ev)
    {
        // Iterate over all entities with GGSolfedTeamComponent and mark them dirty.
        var SolfedComps = AllEntityQuery<GGSolfedTeamComponent>();
        while (SolfedComps.MoveNext(out var uid, out var comp))
        {
            Dirty(uid, comp);
        }
    }
}
