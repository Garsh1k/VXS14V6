using System.Numerics;
using Content.Server.Shuttles.Components;
using Content.Server._VXS.ActiveRadioHeading.Components;
using Content.Shared.Hands;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Shuttles.Components;
using Content.Shared.Trigger;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared._VXS.Manpads.Components;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Server.GameObjects;
using Robust.Server.Audio;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._VXS.Manpads.Systems;

public sealed class VXSManpadsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly GunSystem _gun = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<VXSManpadsLauncherComponent, GotEquippedHandEvent>(OnEquipped);
        SubscribeLocalEvent<VXSManpadsLauncherComponent, GotUnequippedHandEvent>(OnUnequipped);
        SubscribeLocalEvent<VXSManpadsLauncherComponent, ShotAttemptedEvent>(OnShotAttempted);
        SubscribeLocalEvent<VXSManpadsLauncherComponent, AmmoShotEvent>(OnAmmoShot);
        SubscribeLocalEvent<VXSManpadsSpaceExplosionOnlyComponent, AttemptTriggerEvent>(OnAttemptTriggerOutsideSpace);
    }

    public override void Update(float frameTime)
    {
        var defenseQuery = EntityQueryEnumerator<VXSManpadsAutoDefenseComponent, VXSManpadsLauncherComponent, GunComponent, TransformComponent>();
        while (defenseQuery.MoveNext(out var uid, out var defense, out var launcher, out var gun, out var xform))
        {
            if (!defense.Enabled)
                continue;

            if (IsSpaceMap(xform.MapID, launcher.PreferredSpaceMapName))
            {
                launcher.CurrentTarget = null;
                continue;
            }

            if (_timing.CurTime >= defense.NextScan)
            {
                defense.NextScan = _timing.CurTime + TimeSpan.FromSeconds(Math.Max(0.1f, defense.ScanInterval));

                var position = _transform.GetMapCoordinates(uid, xform).Position;
                var target = FindNearestHostileShip(position, launcher, defense.ScanRadius);
                var changed = target != launcher.CurrentTarget;

                launcher.CurrentTarget = target;

                if (changed && target is not null && _timing.CurTime >= launcher.NextLockSound)
                {
                    launcher.NextLockSound = _timing.CurTime + TimeSpan.FromSeconds(Math.Max(0.1f, launcher.LockSoundCooldown));
                    _audio.PlayEntity(launcher.LockSound, Filter.Pvs(uid), uid, true);
                }
            }

            if (_timing.CurTime < defense.NextFire)
                continue;

            var currentTarget = launcher.CurrentTarget;
            if (currentTarget is null || TerminatingOrDeleted(currentTarget.Value))
                continue;

            if (!_gun.AttemptShoot(uid, gun))
                continue;

            defense.NextFire = _timing.CurTime + TimeSpan.FromSeconds(Math.Max(0.1f, defense.FireInterval));
        }

        var launcherQuery = EntityQueryEnumerator<VXSManpadsLauncherComponent>();
        while (launcherQuery.MoveNext(out var uid, out var launcher))
        {
            if (launcher.Holder is not { } holder || TerminatingOrDeleted(holder))
            {
                if (HasComp<VXSManpadsAutoDefenseComponent>(uid))
                    continue;

                launcher.Holder = null;
                launcher.CurrentTarget = null;
                continue;
            }

            if (_timing.CurTime < launcher.NextScan)
                continue;

            launcher.NextScan = _timing.CurTime + TimeSpan.FromSeconds(Math.Max(0.1f, launcher.ScanInterval));

            var holderPos = _transform.GetMapCoordinates(holder).Position;
            var newTarget = FindNearestHostileShip(holderPos, launcher, launcher.ScanRadius);
            var changed = newTarget != launcher.CurrentTarget;

            launcher.CurrentTarget = newTarget;

            if (changed && newTarget is not null && _timing.CurTime >= launcher.NextLockSound)
            {
                launcher.NextLockSound = _timing.CurTime + TimeSpan.FromSeconds(Math.Max(0.1f, launcher.LockSoundCooldown));
                _audio.PlayEntity(launcher.LockSound, Filter.Pvs(uid), uid, true);
            }
        }

        var transferQuery = EntityQueryEnumerator<VXSManpadsProjectileTransferComponent, TransformComponent>();
        while (transferQuery.MoveNext(out var uid, out var transfer, out var xform))
        {
            if (!transfer.PendingTransfer || _timing.CurTime < transfer.TransferAt)
                continue;

            transfer.PendingTransfer = false;

            if (!_map.MapExists(transfer.Destination.MapId))
                continue;

            _transform.SetMapCoordinates(uid, transfer.Destination);
            _transform.AttachToGridOrMap(uid, xform);

            Vector2? transferDirection = null;
            if (transfer.Target is { } transferTarget && !TerminatingOrDeleted(transferTarget))
            {
                var targetCoords = _transform.GetMapCoordinates(transferTarget);
                var toTarget = targetCoords.Position - transfer.Destination.Position;
                if (toTarget.LengthSquared() > 0.0001f)
                {
                    transferDirection = Vector2.Normalize(toTarget);
                    _transform.SetWorldRotation(xform, transferDirection.Value.ToWorldAngle());
                }
            }

            var heading = CompOrNull<VXSActiveThrusterRadioHeadingComponent>(uid);
            if (heading != null && heading.TargetEntity is null && transfer.Target is not null)
                heading.TargetEntity = transfer.Target;

            var body = CompOrNull<PhysicsComponent>(uid);
            if (body != null)
            {
                var speed = 0f;
                var headingComp = CompOrNull<VXSActiveThrusterRadioHeadingComponent>(uid);
                if (headingComp != null)
                    speed = MathF.Max(headingComp.Speed, headingComp.InitialSpeed);

                if (speed > 0f)
                {
                    var direction = transferDirection ?? _transform.GetWorldRotation(xform).ToWorldVec();
                    _physics.SetLinearVelocity(uid, direction * speed, body: body);
                }
            }
        }
    }

    private void OnEquipped(Entity<VXSManpadsLauncherComponent> ent, ref GotEquippedHandEvent args)
    {
        ent.Comp.Holder = args.User;
    }

    private void OnUnequipped(Entity<VXSManpadsLauncherComponent> ent, ref GotUnequippedHandEvent args)
    {
        if (ent.Comp.Holder == args.User)
        {
            ent.Comp.Holder = null;
            ent.Comp.CurrentTarget = null;
        }
    }

    private void OnShotAttempted(Entity<VXSManpadsLauncherComponent> ent, ref ShotAttemptedEvent args)
    {
        if (TerminatingOrDeleted(args.User))
        {
            args.Cancel();
            return;
        }

        var userMap = Transform(args.User).MapID;
        if (IsPlanetMap(userMap))
            return;

        args.Cancel();
        ent.Comp.CurrentTarget = null;
    }

    private void OnAttemptTriggerOutsideSpace(Entity<VXSManpadsSpaceExplosionOnlyComponent> ent, ref AttemptTriggerEvent args)
    {
        var mapId = Transform(ent).MapID;
        if (IsSpaceMap(mapId, null))
            return;

        args.Cancelled = true;
    }

    private void OnAmmoShot(Entity<VXSManpadsLauncherComponent> ent, ref AmmoShotEvent args)
    {
        EntityUid? target = ent.Comp.CurrentTarget;

        if ((target is null || TerminatingOrDeleted(target.Value)) && ent.Comp.Holder is { } holder && !TerminatingOrDeleted(holder))
        {
            var holderPos = _transform.GetMapCoordinates(holder).Position;
            target = FindNearestHostileShip(holderPos, ent.Comp, ent.Comp.ScanRadius);
            ent.Comp.CurrentTarget = target;
        }

        if (target is not { } targetUid || TerminatingOrDeleted(targetUid))
            return;

        var targetXform = Transform(targetUid);

        var targetMap = targetXform.MapID;
        if (!_map.MapExists(targetMap))
            return;

        var launcherMapCoords = _transform.GetMapCoordinates(ent);
        var launcherOnTargetMap = new MapCoordinates(launcherMapCoords.Position, targetMap);

        foreach (var projectile in args.FiredProjectiles)
        {
            var transfer = CompOrNull<VXSManpadsProjectileTransferComponent>(projectile);
            if (transfer == null)
                continue;

            var seconds = transfer.TransferDistance / Math.Max(1f, transfer.TransferSpeed);
            var projectileMapId = Transform(projectile).MapID;

            if (projectileMapId != targetMap)
                seconds = 0.01f;

            seconds = Math.Min(seconds, Math.Max(0.01f, transfer.MaxTransferDelay));

            transfer.PendingTransfer = true;
            transfer.TransferAt = _timing.CurTime + TimeSpan.FromSeconds(Math.Max(0.01f, seconds));
            transfer.Destination = launcherOnTargetMap;
            transfer.Target = targetUid;

            var heading = CompOrNull<VXSActiveThrusterRadioHeadingComponent>(projectile);
            if (heading != null)
                heading.TargetEntity = targetUid;
        }
    }

    private EntityUid? FindNearestHostileShip(Vector2 holderPos, VXSManpadsLauncherComponent launcher, float radius)
    {
        var closestDistSq = float.MaxValue;
        EntityUid? closest = null;

        var query = EntityQueryEnumerator<MapGridComponent, TransformComponent>();
        while (query.MoveNext(out var gridUid, out _, out var gridXform))
        {
            if (!IsSpaceMap(gridXform.MapID, launcher.PreferredSpaceMapName))
                continue;

            if (!HasComp<ShuttleComponent>(gridUid))
                continue;

            if (HasFriendlyTransponder(gridUid, launcher))
                continue;

            var gridPos = _transform.GetMapCoordinates(gridUid, gridXform).Position;
            var distanceSq = Vector2.DistanceSquared(holderPos, gridPos);
            if (distanceSq > radius * radius)
                continue;

            if (distanceSq >= closestDistSq)
                continue;

            closestDistSq = distanceSq;
            closest = gridUid;
        }

        return closest;
    }

    private bool HasFriendlyTransponder(EntityUid gridUid, VXSManpadsLauncherComponent launcher)
    {
        var transponder = CompOrNull<VXSIffTransponderComponent>(gridUid);
        if (transponder != null && launcher.FriendlyTransponders.Contains(transponder.IffType))
            return true;

        var children = new HashSet<Entity<VXSIffTransponderComponent>>();
        _lookup.GetChildEntities(gridUid, children);

        foreach (var child in children)
        {
            if (launcher.FriendlyTransponders.Contains(child.Comp.IffType))
                return true;
        }

        return false;
    }

    private bool IsSpaceMap(MapId mapId, string? preferredName)
    {
        if (!_map.MapExists(mapId))
            return false;

        var mapUid = _map.GetMapOrInvalid(mapId);
        if (HasComp<BiomeComponent>(mapUid))
            return false;

        if (string.IsNullOrWhiteSpace(preferredName))
            return true;

        var meta = CompOrNull<MetaDataComponent>(mapUid);
        return meta != null &&
               string.Equals(meta.EntityName, preferredName, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsPlanetMap(MapId mapId)
    {
        if (!_map.MapExists(mapId))
            return false;

        var mapUid = _map.GetMapOrInvalid(mapId);
        return HasComp<BiomeComponent>(mapUid);
    }
}
