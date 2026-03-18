using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Robust.Shared.Map;
using Content.Shared.Examine;

namespace Content.Shared._VXS14.Rangefinder;

public sealed class SharedRangeFinderSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RangefinderComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(EntityUid uid, RangefinderComponent component, AfterInteractEvent args)
    {
        if (!_examine.InRangeUnOccluded(args.User, args.ClickLocation, SharedInteractionSystem.MaxRaycastRange))
            return;

        var netCoords = new NetCoordinates(EntityManager.GetNetEntity(args.ClickLocation.EntityId), args.ClickLocation.Position);
        var doAfterEvent = new RangefinderDoAfterEvent(netCoords);

        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, component.MeasureDelay,
                                          doAfterEvent, uid)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        args.Handled = true;
    }
}
