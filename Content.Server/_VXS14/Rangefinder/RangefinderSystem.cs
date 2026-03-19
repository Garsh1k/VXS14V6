using Content.Server.Chat.Managers;
using Content.Shared._VXS14.Rangefinder;
using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.Server._VXS14.Rangefinder;

public sealed class RangefinderSystem : EntitySystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RangefinderComponent, RangefinderDoAfterEvent>(OnDoAfterCompleted);
    }

    private void OnDoAfterCompleted(EntityUid uid, RangefinderComponent component, RangefinderDoAfterEvent args)
    {
        if (args.Cancelled || !TryComp<ActorComponent>(args.User, out var actor))
            return;

        var targetEntity = EntityManager.GetEntity(args.Coordinates.NetEntity);
        var targetCoords = new EntityCoordinates(targetEntity, args.Coordinates.Position);
        var userCoords = Transform(args.User).Coordinates;

        var diff = targetCoords.Position - userCoords.Position;
        var distanceX = (int) MathF.Abs(diff.X);
        var distanceY = (int) MathF.Abs(diff.Y);

        var message = Loc.GetString("rangefinder-chat-distance", ("x", distanceX), ("y", distanceY));
        _chatManager.DispatchServerMessage(actor.PlayerSession, message);
    }
}
