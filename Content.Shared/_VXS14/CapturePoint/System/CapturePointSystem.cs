using System.Collections.Generic;
using Content.Shared.GG.CapturePoint;
using Robust.Shared.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using System.Linq;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Server.GG.CapturePoint;

public partial class SharedGGCapturePointSystem : EntitySystem
{
    private const float CaptureRate = 20f; // Progress per second when capturing
    private const float DecayRate = 20f; // Progress per second when decaying

    public override void Initialize()
    {
        base.Initialize();

    }

    protected void OnGetState(EntityUid uid, GGCapturePointComponent component, ref ComponentGetState args)
    {
        if(component.Team == null)
            return;
        args.State = new GGCapturePointComponentState(
            component.CurrentPointProgression,
            component.Leader,
            component.Team);
    }

    public void DecayPoint(GGCapturePointComponent capturePoint, float frameTime)
    {
        capturePoint.CurrentPointProgression = MathF.Max(0, capturePoint.CurrentPointProgression - DecayRate * frameTime);
        if (capturePoint.CurrentPointProgression == 0)
        {
            capturePoint.Leader = "None";
        }
        Dirty(capturePoint.Owner, capturePoint);
    }

    public void NeutralizePoint(GGCapturePointComponent capturePoint, float frameTime)
    {
        capturePoint.CurrentPointProgression = MathF.Max(0, capturePoint.CurrentPointProgression - DecayRate * frameTime);
        if (capturePoint.CurrentPointProgression == 0)
        {
            capturePoint.Leader = "None";
        }
        Dirty(capturePoint.Owner, capturePoint);
    }


}
