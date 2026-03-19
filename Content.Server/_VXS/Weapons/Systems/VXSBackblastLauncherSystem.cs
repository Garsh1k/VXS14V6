using System.Numerics;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared._VXS.Weapons.Components;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;

namespace Content.Server._VXS.Weapons.Systems;

public sealed class VXSBackblastLauncherSystem : EntitySystem
{
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<VXSBackblastLauncherComponent, AmmoShotEvent>(OnAmmoShot);
    }

    private void OnAmmoShot(Entity<VXSBackblastLauncherComponent> ent, ref AmmoShotEvent args)
    {
        if (args.FiredProjectiles.Count == 0)
            return;

        var forwardDirection = GetForwardDirection(args.FiredProjectiles);
        if (forwardDirection.LengthSquared() <= 0.0001f)
            return;

        var shooter = GetShooter(args.FiredProjectiles);
        var gunVelocity = TryComp<PhysicsComponent>(ent, out var gunBody)
            ? gunBody.LinearVelocity
            : Vector2.Zero;

        var backblast = Spawn(ent.Comp.ProjectileProto, Transform(ent).Coordinates);
        var backDirection = -Vector2.Normalize(forwardDirection);
        _gun.ShootProjectile(backblast, backDirection, gunVelocity, ent, shooter, ent.Comp.Speed);
    }

    private Vector2 GetForwardDirection(List<EntityUid> firedProjectiles)
    {
        foreach (var projectile in firedProjectiles)
        {
            if (TerminatingOrDeleted(projectile))
                continue;

            if (TryComp<PhysicsComponent>(projectile, out var projectileBody) &&
                projectileBody.LinearVelocity.LengthSquared() > 0.0001f)
            {
                return projectileBody.LinearVelocity;
            }

            return _transform.GetWorldRotation(projectile).ToWorldVec();
        }

        return Vector2.Zero;
    }

    private EntityUid? GetShooter(List<EntityUid> firedProjectiles)
    {
        foreach (var projectile in firedProjectiles)
        {
            if (TryComp<ProjectileComponent>(projectile, out var projectileComp) && projectileComp.Shooter is { } shooter)
                return shooter;
        }

        return null;
    }
}
